using System;
using CommandLine;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using fastdee.Devices.Udp;
using System.Net;
using System.Net.Sockets;

[assembly: InternalsVisibleTo("fastdee.Tests")]

namespace fastdee
{
    partial class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Parser.Default.ParseArguments<ConnectArgs, SimulateArgs>(args).MapResult(
                (ConnectArgs options) => MainWithParsedAsync(options),
                (SimulateArgs options) => SimulateWithParsedAsync(options),
                _ => Task.FromResult(-1));
        }

        static async Task<int> MainWithParsedAsync(ConnectArgs options)
        {
            try
            {
                var stratHelp = InstantiateConnector(options.Algorithm, options.DifficultyMultiplier);
                var serverInfo = FromArgs(options);
                return await MainWithParsedAsync(serverInfo, stratHelp);
            }
            catch (BadInitializationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return -3;
            }
        }
        private static ServerConnectionInfo FromArgs(ConnectArgs options)
        {
            // Pool server needs some additional parsing while I grok the documentation and find out if the lib can parse for me.
            string poolurl;
            ushort poolport;
            {
                var parts = options.Pool.Split(':');
                if (parts.Length != 2) throw new BadInitializationException("Does not look like valid POOL:PORT endpoint!");
                poolurl = parts[0];
                poolport = ushort.Parse(parts[1]);
            }
            var presentingAs = options.SubscribeAs ?? MyCanonicalSubscription();
            return new ServerConnectionInfo(poolurl, poolport, presentingAs, options.UserName, options.WorkerName, options.SillyPassword);
        }

        static async Task<int> MainWithParsedAsync(ServerConnectionInfo serverInfo, Stratum.Connector stratHelp)
        {
            var tracker = new Devices.Tracker<IPEndPoint>(stratHelp.GenWork);
            using var cts = new System.Threading.CancellationTokenSource();
            var stratum = new Stratificator(stratHelp);
            using var orchestrator = new Orchestrator(cts.Token);
            orchestrator.NonceFound += (src, ev) =>
            {
                var work = tracker.RetrieveOriginal(ev.workid);
                if (null != work)
                {
                    var nonce = (uint)(work.nonceBase + ev.increment);
                    // TODO: check hash and diff maybe discard / signal error
                    stratum.Submit(work.info, nonce);
                }
            };
            BindOrThrow(orchestrator);
            await Task.WhenAll(
                stratum.PumpForeverAsync(serverInfo, cts.Token),
                orchestrator.RunAsync(tracker.ConsumeNonces)
            );
            return -1024; // in theory, you should not be reaching me
        }

        static Stratum.Connector InstantiateConnector(string algorithm, double? diffmul, ulong? n2off = null, ulong nonceStart = 0)
        {
            var initialMerkle = ChooseMerkleGenerator(algorithm);
            if (null == initialMerkle) throw new BadInitializationException($"Unsupported algorithm: {algorithm}");
            var factors = ChooseDifficulties(algorithm, diffmul);
            var difficultyCalculator = new LockingCurrentDifficulty(ChooseDiffMaker(algorithm, factors));
            var headerGen = new Stratum.HeaderGenerator(initialMerkle);
            if (n2off.HasValue) headerGen.NextNonce(n2off.Value);
            return new Stratum.Connector(headerGen, difficultyCalculator);
        }

        static string MyCanonicalSubscription()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version;
            if (null == version) // apparently this can happen with single-file assembly bundles or whatever they are called
            {
                return "fastdee";
            }
            var major = (uint)version.Major;
            var minor = (uint)version.Minor;
            var patch = (uint)version.Build;
            return $"fastdee/{major}.{minor}.{patch}";
        }

        static internal Stratum.HeaderGenerator.FromCoinbaseFunc? ChooseMerkleGenerator(string algo) => algo.ToLowerInvariant() switch
        {
            "keccak" => (coinbase) => PoolOps.Merkles.SingleSha(coinbase),
            _ => null
        };

        internal static DifficultyMultipliers ChooseDifficulties(string algo, double? desired = null)
        {
            var diffy = ChooseTypicalDifficulties(algo);
            if (false == desired.HasValue) return diffy;
            return new DifficultyMultipliers()
            {
                Stratum = desired.Value,
                One = diffy.One,
                Share = diffy.Share
            };
        }

        internal static DifficultyMultipliers ChooseTypicalDifficulties(string algo) => algo.ToLowerInvariant() switch
        {
            "keccak" => new DifficultyMultipliers()
            {
                Stratum = 1,
                One = 256,
                Share = 1
            },
            _ => throw new NotImplementedException()
        };

        internal static IDifficultyCalculation ChooseDiffMaker(string algo, DifficultyMultipliers mults) => algo.ToLowerInvariant() switch
            {
                "keccak" => new BtcLikeDifficulty(mults.Stratum, mults.One),
                _ => throw new NotImplementedException()
            };

        /// <summary>
        /// Convenience function to bind the orchestrator to a port.
        /// </summary>
        /// <remarks>Could this be in the object itself? I'd rather not build a dependancy to the exception.</remarks>
        private static void BindOrThrow(Orchestrator orchestrator)
        {
            try
            {
                orchestrator.Bind(new IPEndPoint(IPAddress.Any, 18458));
            }
            catch (SocketException ex)
            {
                throw new BadInitializationException($"Failed to setup socket, OS error: {ex.ErrorCode}");
                // ^ Not quite, this comes very late and it doesn't even depend on the input data but anyway...
            }
        }
    }
}
