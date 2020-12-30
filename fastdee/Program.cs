using System;
using CommandLine;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("fastdee.Tests")]

namespace fastdee
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Args>(args).MapResult(
                options => MainWithParsed(options),
                _ => -1);
        }

        static int MainWithParsed(Args options)
        {
            // Pool server needs some additional parsing while I grok the documentation and find out if the lib can parse for me.
            string poolurl;
            ushort poolport;
            {
                var parts = options.Pool.Split(':');
                if (parts.Length != 2) throw new ApplicationException("Does not look like valid POOL:PORT endpoint!");
                poolurl = parts[0];
                poolport = ushort.Parse(parts[1]);
            }
            var presentingAs = options.SubscribeAs ?? MyCanonicalSubscription();
            var serverInfo = new ServerConnectionInfo(poolurl, poolport, presentingAs, options.UserName, options.WorkerName, options.SillyPassword);
            var merkleGenerator = ChooseMerkleGenerator(options.Algorithm);
            if (null == merkleGenerator)
            {
                Console.Error.WriteLine($"Unsupported algorithm: {options.Algorithm}");
                return -3;
            }
            new Stratificator(serverInfo, merkleGenerator).PumpForeverAsync().Wait(); // TODO: the other services
            return -2;
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

        static internal WorkGenerator.FromCoinbaseFunc? ChooseMerkleGenerator(string algo) => algo.ToLowerInvariant() switch
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

        internal static DifficultyTarget MakeDiffTarget(string algo, DifficultyMultipliers mults, double serverReq) {
            var shareDiff = mults.Stratum * serverReq;
            if (shareDiff <= 0.0) throw new ArgumentException(nameof(shareDiff));
            return algo.ToLowerInvariant() switch
            {
                "keccak" => BtcLikeDiffTarget(shareDiff, mults.One),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Difficulty target bits calculation used by most coins.
        /// </summary>
        /// <remarks>
        /// Before you wonder, the donor project here is M8M, AbstractWorkSource.cpp::186.
        /// MIT license.
        /// </remarks>
        static DifficultyTarget BtcLikeDiffTarget(double diff, double diffOneMul)
        {
            /*
            Ok, there's this constant, "truediffone" which is specified as a 256-bit value
            0x00000000FFFF0000000000000000000000000000000000000000000000000000
                          |------------------- 52 zeros --------------------|
            So it's basically aushort(0xFFFF) << (52 * 4)
            Or: 65535 * ... 2^208?
            Legacy miners have those values set up, so they can go use double-float division to effectively
            expand the bit representation and select the bits they care. By using multiple passes, they pull
            out successive ranges of reductions. They use the following constants:
            truediffone = 0x00000000FFFF0000000000000000000000000000000000000000000000000000
            bits192     = 0x0000000000000001000000000000000000000000000000000000000000000000
            bits128     = 0x0000000000000000000000000000000100000000000000000000000000000000
            bits64      = 0x0000000000000000000000000000000000000000000000010000000000000000
            Because all those integers have a reduced range, they can be accurately represented by a double.
            See diffCalc.html for a large-integer testing framework. */
            const double BITS_192 = 6277101735386680763835789423207666416102355444464034512896.0;
            const double BITS_128 = 340282366920938463463374607431768211456.0;
            const double BITS_64 = 18446744073709551616.0;

            double big = (diffOneMul * TRUE_DIFF_ONE) / diff;
            Span<double> k = stackalloc double[4] { BITS_192, BITS_128, BITS_64, 1 };
            Span<ulong> target = stackalloc ulong[4];
            for (var loop = 0; loop < 4; loop++)
            {
                double partial = big / k[loop];
                ulong magic = (ulong)partial;
                target[4 - loop - 1] = magic;
                // ^ note: both legacy and M8M here force this little endian. I don't.
                // ^ Endianess is something to care about when mix-matching with byte sequences but I plan to stay away this time.
                partial = magic * k[loop];
                big -= partial;
            }
            return new DifficultyTarget()
            {
                ShareDiff = diff,
                TargA = target[0],
                TargB = target[1],
                TargC = target[2],
                TargD = target[3]
            };
        }

        const double TRUE_DIFF_ONE = 26959535291011309493156476344723991336010898738574164086137773096960.0;
    }
}
