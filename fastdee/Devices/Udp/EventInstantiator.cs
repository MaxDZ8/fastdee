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
        internal static TurnOnArgs<IPEndPoint> Hello(IPEndPoint origin, ReadOnlySpan<byte> buff)
        {
            buff = LoadOps.LoadByte(buff, out var unilen);
            buff = LoadOps.LoadByte(buff, out var devlen);
            buff = LoadOps.LoadByteArr(buff, unilen, out var ident);
            buff = LoadOps.LoadByteArr(buff, devlen, out var devspec);
            ThrowIfNotFullyConsumed(buff);
            return new TurnOnArgs<IPEndPoint>(origin, ident, devspec);
        }

        internal static WorkRequestArgs<IPEndPoint> GimmeWork(IPEndPoint origin, ReadOnlySpan<byte> buff)
        {
            buff = LoadOps.LoadUshort(buff, out var kind);
            buff = LoadOps.LoadUint(buff, out var reservation);
            ThrowIfNotFullyConsumed(buff);
            var format = (WireAlgoFormat)kind;
            return new WorkRequestArgs<IPEndPoint>(origin, format, reservation);
        }

        internal static NonceFoundArgs FoundNonce(ReadOnlySpan<byte> buff)
        {
            buff = LoadOps.LoadUint(buff, out var wid);
            buff = LoadOps.LoadUlong(buff, out var offset);
            buff = LoadOps.LoadByte(buff, out var uintCount);
            buff = LoadOps.LoadByteArr(buff, uintCount * 4, out var hash);
            ThrowIfNotFullyConsumed(buff);
            return new NonceFoundArgs(wid, offset, hash);
        }

        static void ThrowIfNotFullyConsumed(ReadOnlySpan<byte> buff)
        {
            if (buff.IsEmpty) return;
            throw new ArgumentException($"{buff.Length} bytes not consumed", nameof(buff));
        }
    }
}
