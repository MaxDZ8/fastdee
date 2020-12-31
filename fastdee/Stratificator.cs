using System;
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
    class Stratificator
    {
        readonly Stratum.ICallbacks callbacks;
        bool traffic;

        internal Stratificator(Stratum.ICallbacks callbacks)
        {
            this.callbacks = callbacks;
        }

        internal async Task PumpForeverAsync(ServerConnectionInfo serverInfo)
        {
            while (true)
            {
                try
                {
                    await PumpConnectionAsync(serverInfo);
                }
                catch
                {
                    // Yeah, suppress all. I really mean it.
                    if (!traffic) return;
                }
                await Task.Delay(60_000); // those errors usually don't go away easily.
            }
        }

        async Task PumpConnectionAsync(ServerConnectionInfo serverInfo)
        {
            var addr = Dns.GetHostAddresses(serverInfo.poolurl)[0];
            var endpoint = new IPEndPoint(addr, serverInfo.poolport);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            callbacks.Connecting();
            await socket.ConnectAsync(endpoint);
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
            await Task.Delay(-1); // can I do anything more useful with it?
        }
    }
}
