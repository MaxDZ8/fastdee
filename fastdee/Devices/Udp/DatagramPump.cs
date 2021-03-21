using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// Yet another "pumper" class which spawns a task pumping up datagrams from a shared socket.
    /// In this case 'pumping' also means elevating to a structured event, not just getting buffers,
    /// mere socket pumping is too easy!
    /// 
    /// NETCORE has, from a few versions an easy-going <see cref="UdpClient"/> class taking care of wrapping
    /// a socket with convenience methods. There are a few quirks with it with most of them related to the way
    /// receive works i.e. without support for built-in-cancellation. Or maybe yes.
    /// The lack of cancellation tokens for Sockets calls get discussed over and over; it is not clear to me
    /// whatever <see cref="UdpClient"/> is included in the discussion.
    /// 
    /// There are further issues with <see cref="UdpClient"/> which I could let pass such us
    /// - it tends to alloc more than I want instead of reusing a given parse buffer
    /// - you enable/disable socket options after creation such as nagle and timeout, which again,
    ///   I could live with except timeout does not seem to apply to async calls.
    /// 
    /// Again, I'm not at ease with those patterns but I could accept them.
    /// What I don't accept is the idea of just spawning Tasks like crazy and joining them
    /// to <see cref="Task.WhenAny(System.Collections.Generic.IEnumerable{Task})"/> to emulate timeouts.
    /// 
    /// At that point, I'm not even sure what value <see cref="UdpClient"/> even provides so
    /// I'll be sticking with <see cref="Socket"/> which, by the way supports cancellation "out of the box"
    /// as in the sense of being there by extension methods but will hopefully be promoted at some point
    /// (https://github.com/dotnet/runtime/issues/33417)
    /// </summary>
    class DatagramPump : ICommunicationsSource<IPEndPoint>
    {
        readonly Socket source;
        readonly CancellationToken cancel;

        public event EventHandler<WorkRequestArgs<IPEndPoint>>? WorkAsked;
        public event EventHandler<NonceFoundArgs>? NonceFound;

        /// <summary>
        /// This is a bit special because it's not really part of the communication protocol we're mostly interested in.
        /// It is part of the protocol for UDP but not really for work management, UDP devices announce themselves so the
        /// orchestrator can inform them about the IP address to use.
        /// </summary>
        public event EventHandler<TurnOnArgs<IPEndPoint>>? IntroducedItself;

        /// <summary>
        /// I will work using those two object. Creation and management is up to you.
        /// </summary>
        /// <param name="source">This must be an IPv4 UDP socket. I will only read.</param>
        /// <param name="cancel">How to make me quit. Again, responsability is yours!</param>
        internal DatagramPump(Socket source, CancellationToken cancel)
        {
            this.source = source;
            this.cancel = cancel;
        }

        internal async Task ReceiveForeverAsync()
        {
            var buffer = new byte[4096]; // TODO: in line of theory I should support packets bigger than that. But I don't.
            var originator = new IPEndPoint(0, 0);
            while (false == cancel.IsCancellationRequested)
            {
                try
                {
                    var res = await source.ReceiveMessageFromAsync(buffer, SocketFlags.None, originator);
                    if (res.ReceivedBytes == buffer.Length) throw new NotImplementedException("UDP packets must be less than 4096 bytes!");
                    Process(buffer, res);
                }
                catch(Exception ex)
                {
                    // Just suppress it. In the future I'll need to choose a logging library and decide what to do.
                    // (but for the time being, dumping to console doesn't hurt)
                    Console.WriteLine("Uncaught exception in datagram pump v v v v v v v");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^ ^,");
                    break;
                }
            }
        }

        void Process(Span<byte> octects, SocketReceiveMessageFromResult msg)
        {
            if (msg.ReceivedBytes < 1) return; // I have no command type to parse... uh!
            octects = octects[..msg.ReceivedBytes];
            if (msg.RemoteEndPoint is not IPEndPoint originator)
            {
                // todo. I don't like this at all.
                var baddie = msg.RemoteEndPoint.GetType().Name;
                throw new NotImplementedException($"UDP from unsupported endpoint {baddie}");
            }
            var kind = (PacketKind)octects[0];
            var rem = octects[1..];
            switch (kind)
            {
                case PacketKind.HelloOnline:
                    {
                        var got = EventInstantiator.Hello(originator, rem);
                        IntroducedItself?.Invoke(this, got);
                        break;
                    }
                case PacketKind.GimmeWork:
                    {
                        var got = EventInstantiator.GimmeWork(originator, rem);
                        WorkAsked?.Invoke(this, got);
                        break;
                    }
                case PacketKind.FoundNonce:
                    {
                        var got = EventInstantiator.FoundNonce(rem);
                        NonceFound?.Invoke(this, got);
                        break;
                    }
            }
            // unknown? shall log! TODO
        }
    }
}
