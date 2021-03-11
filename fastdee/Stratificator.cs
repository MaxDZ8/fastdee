using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace fastdee
{
    /// <summary>
    /// Encapsulates a FSM taking care of connecting and transacting to a server.
    /// The whole point is other concurrent parts of the program can access me and ask how it is going.
    /// Yes, you can think of it as a state machine... or not.
    /// </summary>
    class Stratificator : IDisposable
    {
        readonly Stratum.ICallbacks callbacks;
        bool traffic;
        readonly BlockingCollection<CandidateNonce> pending = new BlockingCollection<CandidateNonce>();

        internal Stratificator(Stratum.ICallbacks callbacks)
        {
            this.callbacks = callbacks;
        }

        internal async Task PumpForeverAsync(ServerConnectionInfo serverInfo, System.Threading.CancellationToken goodbye)
        {
            while (true)
            {
                try
                {
                    await PumpConnectionAsync(serverInfo, goodbye);
                }
                catch
                {
                    // Yeah, suppress all. I really mean it.
                    if (!traffic) return;
                }
                await Task.Delay(60_000, goodbye); // those errors usually don't go away easily.
            }
        }

        async Task PumpConnectionAsync(ServerConnectionInfo serverInfo, System.Threading.CancellationToken goodbye)
        {
            var addr = Dns.GetHostAddresses(serverInfo.poolurl)[0];
            var endpoint = new IPEndPoint(addr, serverInfo.poolport);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            callbacks.Connecting();
            await socket.ConnectAsync(endpoint, goodbye);
            // Since I initiate the stratum communication, I can take it easy there.
            // Nothing will come before I send anything.
            using var pumper = new SocketPipelinesLineChannel(socket);
            using var continuator = new Stratum.StratumContinuator(pumper.WriteAsync);
            var notificator = new Stratum.NotificationSystem();
            notificator.NewJobReceived += (_, ev) => callbacks.StartNewJob(ev.newJob);
            notificator.DifficultyReceived += (_, ev) => callbacks.SetDifficulty(ev.difficulty);
            pumper.GottaLine += (src, ev) =>
            {
                var stuff = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonRpc.Message>(ev.payload);
                if (stuff.id.HasValue) continuator.MangleReply(stuff.id.Value, stuff.rawRes, stuff.rawErr);
                else if (stuff.method != null) {
                    if (false == notificator.Mangle(stuff.method, stuff.evargs))
                    {
                        // TODO: log this
                    }
                }
                else
                {
                    // Just drop it. Should I be logging? Giving up? Closing?
                }
            };

            callbacks.Subscribing();
            var subscribed = await continuator.SubscribeAsync(serverInfo.presentingAs);
            traffic = true;
            callbacks.Subscribed(subscribed);
            var authorized = await continuator.AuthorizeAsync(serverInfo.userName, serverInfo.workerName, serverInfo.sillyPassword);
            callbacks.Authorized(authorized);
            var signature = $"{serverInfo.userName}.{serverInfo.workerName}";
            while (!goodbye.IsCancellationRequested)
            {
                if (pending.TryTake(out var share, TimeSpan.FromMilliseconds(20))) SubmitDetached(signature, share, continuator);
            }
        }

        /// <summary>
        /// Sends a nonce to the server. The sending itself is awaited to complete.
        /// I don't wait for server response, but I set up everything required to track the result.
        /// </summary>
        static private void SubmitDetached(string userName, CandidateNonce share, Stratum.StratumContinuator continuator)
        {
            Task.Run(async () =>
            {
                try
                {
                    var accepted = await continuator.SubmitAsync(userName, share.JobId, share.NetworkTime, share.Nonce2, share.Nonce);
                    if (accepted) Console.WriteLine($"OK ACCEPTED {share.Nonce:x8}");
                    else Console.WriteLine($"OK REJECTED {share.Nonce:x8}");
                }
                catch(Exception ex)
                {
                    // For the time being, suppress everything, the Task must not fail!
                    Console.WriteLine(ex.Message);
                }
            });
        }

        /// <summary>
        /// Enqueues a nonce to be sent to server. 
        /// </summary>
        internal void Submit(Stratum.ShareSubmitInfo source, uint nonce)
            => pending.Add(new CandidateNonce(source.JobId, source.Nonce2, source.NetworkTime, nonce));

        public void Dispose()
        {
            ((IDisposable)pending).Dispose();
        }

        class CandidateNonce
        {
            internal string JobId { get; }
            internal IReadOnlyList<byte> Nonce2 { get; }
            internal IReadOnlyList<byte> NetworkTime { get; }
            internal uint Nonce { get; }

            internal CandidateNonce(string jobid, IReadOnlyList<byte> nonce2, IReadOnlyList<byte> ntime, uint nonce)
            {
                JobId = jobid;
                Nonce2 = nonce2;
                NetworkTime = ntime;
                Nonce = nonce;
            }
        }
    }
}
