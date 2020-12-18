﻿using System.Net;
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
        readonly ServerConnectionInfo serverInfo;
        readonly ThreadShared delicate = new ThreadShared();

        class ThreadShared
        {
            public StratumState state;
            public bool alive;
        }

        internal Stratificator(ServerConnectionInfo serverInfo)
        {
            this.serverInfo = serverInfo;
        }

        internal async Task PumpForeverAsync()
        {
            while (true)
            {
                try
                {
                    await PumpConnectionAsync();
                }
                catch
                {
                    // Yeah, suppress all. I really mean it.
                    if (!Alive) return;
                    lock (delicate) delicate.state = StratumState.Failed;
                }
                await Task.Delay(60_000); // those errors usually don't go away easily.
            }
        }

        async Task PumpConnectionAsync()
        {
            var addr = Dns.GetHostAddresses(serverInfo.poolurl)[0];
            var endpoint = new IPEndPoint(addr, serverInfo.poolport);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            lock (delicate) delicate.state = StratumState.Connecting;
            await socket.ConnectAsync(endpoint);
            // Since I initiate the stratum communication, I can take it easy there.
            // Nothing will come before I send anything.
            using var pumper = new SocketPipelinesLineChannel(socket);
            using var continuator = new Stratum.StratumContinuator(pumper.WriteAsync);
            var notificator = new Stratum.NotificationSystem();
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

            lock (delicate) delicate.state = StratumState.Subscribing;
            var subscribed = await continuator.SubscribeAsync(serverInfo.presentingAs);
            lock (delicate)
            {
                delicate.state = StratumState.Authorizing;
                delicate.alive = true;
            }
            var authorized = await continuator.AuthorizeAsync(serverInfo.userName, serverInfo.workerName, serverInfo.sillyPassword);
            if (false == authorized) throw new BadStratumAuthException();
            System.Console.WriteLine("OK Authorized");
            await Task.Delay(-1); // can I do anything more useful with it?
        }

        public StratumState Status
        {
            get
            {
                lock (delicate) return delicate.state;
            }
        }
        /// <summary>
        /// True if we connected to a server which gave us at least a reply.
        /// </summary>
        public bool Alive
        {
            get
            {
                lock (delicate) return delicate.alive;
            }
        }
    }
}
