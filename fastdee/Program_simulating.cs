using System;
using System.Collections.Generic;
using fastdee.Stratum;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using fastdee.Devices.Udp;

namespace fastdee
{
    partial class Program
    {
        static async Task<int> SimulateWithParsedAsync(SimulateArgs options)
        {
            try
            {
                var load = await LoadKnownWorkAsync(options.Source);
                var stratHelp = InstantiateConnector(load.algo, null, load.nonce2off, load.nonceStart);
                return await SimulateWithParsedAsync(load, stratHelp);
            }
            catch (IOException)
            {
                Console.Error.WriteLine($"Error trying to read: {options.Source}");
                Console.Error.Write($"I am executing from: {Environment.CurrentDirectory}");
                return -3;
            }
            catch (BadInitializationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return -3;
            }
        }

        static async Task<int> SimulateWithParsedAsync(ReplicationData known, Connector stratHelp)
        {
            var subscribeReply = MakeSubscribe(known.subscribe);
            var notifyJob = MakeJob(known.job);
            var shareDiff = GoodDiffOrThrow(known.shareDiff);
            // I don't need to pump anything stratum-side, what I need to do is to just trigger the callbacks with the data.
            stratHelp.Connecting();
            stratHelp.Subscribing();
            stratHelp.Subscribed(subscribeReply);
            stratHelp.Authorized(true);
            stratHelp.StartNewJob(notifyJob);
            stratHelp.SetDifficulty(shareDiff);
            stratHelp.StartingNonce(known.nonceStart);
            using var cts = new System.Threading.CancellationTokenSource();
            using var orchestrator = new Orchestrator(stratHelp.GenWork, cts.Token);
            try
            {
                orchestrator.Bind(new IPEndPoint(IPAddress.Any, 18458));
            }
            catch (SocketException ex)
            {
                Console.Error.WriteLine($"Failed to setup socket, OS error: {ex.ErrorCode}");
                return -7;
            }
            await orchestrator.RunAsync();
            return 0;
        }

        static async Task<ReplicationData> LoadKnownWorkAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            var load = Newtonsoft.Json.JsonConvert.DeserializeObject<ReplicationData>(json);
            if (null == load.subscribe) throw new BadInitializationException("No subscribe information");
            if (null == load.job) throw new BadInitializationException("No job information");
            return load;
        }

        static Stratum.Response.MiningSubscribe MakeSubscribe(ReplicationData.Subscribe repl)
        {
            var nonce1 = HexHelp.DecodeHex(repl.extraNonce1);
            var nonce2sz = GoodNonce2SizeOrThrow(repl.nonce2sz);
            return new Stratum.Response.MiningSubscribe("abcdef0123456789", nonce1, nonce2sz);
        }

        static Stratum.Notification.NewJob MakeJob(ReplicationData.Job repl)
        {
            var jobid = "abcd";
            var cbHead = HexHelp.DecodeHex(repl.cbHead);
            var cbTail = HexHelp.DecodeHex(repl.cbTail);
            var parsedMerkle = DecodeMerkles(repl.merkles);
            var bver = HexHelp.DecodeHex(repl.blockVersion);
            var ndiff = HexHelp.DecodeHex(repl.networkDiff);
            var ntime = HexHelp.DecodeHex(repl.networkTime);
            var res = new Stratum.Notification.NewJob(jobid, bver, null, cbHead, cbTail, ndiff, ntime, true);
            HexHelp.DecodeInto(res.prevBlock.blob, repl.prevHash);
            res.merkles.AddRange(parsedMerkle);
            return res;
        }

        static List<Mining.Merkle> DecodeMerkles(string[]? merkles)
        {
            var res = new List<Mining.Merkle>();
            if (null == merkles) return res;
            for (var loop = 0; loop < merkles.Length; loop++)
            {
                var gen = new Mining.Merkle();
                HexHelp.DecodeInto(gen.blob, merkles[loop]);
                res.Add(gen);
            }
            return res;
        }

        static double GoodDiffOrThrow(double shareDiff)
        {
            if (shareDiff <= .0) throw new ArgumentException("diff must be bigger than 0", nameof(shareDiff));
            return shareDiff;
        }

        static ushort GoodNonce2SizeOrThrow(ulong sz)
        {
            if (sz != 4) throw new ArgumentException("only supported n2 bytecount is 4", nameof(sz));
            return 4;
        }
    }
}
