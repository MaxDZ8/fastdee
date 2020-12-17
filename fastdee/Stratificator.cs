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
        readonly string poolurl;
        readonly ushort port;
        readonly ThreadShared delicate = new ThreadShared();

        class ThreadShared
        {
            public StratumState state;
            public bool alive;
        }

        internal Stratificator(string poolurl, ushort port)
        {
            this.poolurl = poolurl;
            this.port = port;
        }

        internal async Task PumpForeverAsync(string presentingAs)
        {
            while (true)
            {
                try
                {
                    await PumpConnectionAsync(presentingAs);
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

        async Task PumpConnectionAsync(string presentingAs)
        {
            var addr = Dns.GetHostAddresses(poolurl)[0];
            var endpoint = new IPEndPoint(addr, port);
            using var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            lock (delicate) delicate.state = StratumState.Connecting;
            await socket.ConnectAsync(endpoint);
            // Since I initiate the stratum communication, I can take it easy there.
            // Nothing will come before I send anything.
            using var pumper = new SocketPipelinesLineChannel(socket);
            using var continuator = new Stratum.StratumContinuator(pumper.WriteAsync);
            pumper.GottaLine += (src, ev) =>
            {
                var stuff = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonRpc.Message>(ev.payload);
                if (stuff.id.HasValue) continuator.MangleReply(stuff.id.Value, stuff.rawRes, stuff.rawErr);
                else if (stuff.method != null)
                { // native notification
                    throw new System.NotImplementedException();
                }
                else
                {
                    // Just drop it. Should I be logging? Giving up? Closing?
                }
            };

            lock (delicate) delicate.state = StratumState.Subscribing;
            var subscribed = await continuator.SubscribeAsync(presentingAs);
            lock (delicate)
            {
                delicate.state = StratumState.Authorizing;
                delicate.alive = true;
            }
            throw new System.NotImplementedException();
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
