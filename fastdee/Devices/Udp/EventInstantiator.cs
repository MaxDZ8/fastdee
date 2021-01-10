using System;
using System.Net;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// De-serialize from binary into events.
    /// Yes, I considered using flatbuffer and others but after dealing with the devtools of some embedded platforms
    /// I have decided they're not worth the effort yet.
    /// </summary>
    class EventInstantiator
    {
        internal static TurnOnArgs<IPEndPoint> Hello(IPEndPoint origin, Span<byte> rem)
        {
            throw new NotImplementedException();
        }

        internal static WorkRequestArgs<IPEndPoint> GimmeWork(IPEndPoint origin, Span<byte> rem)
        {
            throw new NotImplementedException();
        }

        internal static NonceFoundArgs FoundNonce(Span<byte> rem)
        {
            throw new NotImplementedException();
        }
    }
}
