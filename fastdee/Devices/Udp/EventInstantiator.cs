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
        internal static TurnOnArgs<IPEndPoint> Hello(IPEndPoint origin, Span<byte> buff)
        {
            buff = LoadByte(buff, out var unilen);
            buff = LoadByte(buff, out var devlen);
            buff = LoadByteArr(buff, unilen, out var ident);
            buff = LoadByteArr(buff, devlen, out var devspec);
            ThrowIfNotFullyConsumed(buff);
            return new TurnOnArgs<IPEndPoint>(origin, ident, devspec);
        }

        internal static WorkRequestArgs<IPEndPoint> GimmeWork(IPEndPoint origin, Span<byte> buff)
        {
            buff = LoadUshort(buff, out var kind);
            buff = LoadUlong(buff, out var reservation);
            ThrowIfNotFullyConsumed(buff);
            var format = (WireAlgoFormat)kind;
            return new WorkRequestArgs<IPEndPoint>(origin, format, reservation);
        }

        internal static NonceFoundArgs FoundNonce(Span<byte> buff)
        {
            buff = LoadUint(buff, out var wid);
            buff = LoadUlong(buff, out var offset);
            buff = LoadByte(buff, out var uintCount);
            buff = LoadByteArr(buff, uintCount * 4, out var hash);
            ThrowIfNotFullyConsumed(buff);
            return new NonceFoundArgs(wid, offset, hash);
        }

        static Span<byte> LoadByte(Span<byte> buff, out byte value)
        {
            value = buff[0];
            return buff[1..];
        }

        static Span<byte> LoadByteArr(Span<byte> buff, int elements, out byte[] value)
        {
            value = 0 == elements ? Array.Empty<byte>() : new byte[elements];
            return LoadByteArr(buff, value);
        }

        static Span<byte> LoadByteArr(Span<byte> buff, byte[] value)
        {
            for (var cp = 0; cp < value.Length; cp++) value[cp] = buff[cp];
            return buff[value.Length..];
        }

        static Span<byte> LoadUshort(Span<byte> buff, out ushort value)
        {
            value = buff[1];
            value <<= 8;
            value |= buff[0];
            return buff[2..];
        }

        static Span<byte> LoadUint(Span<byte> buff, out uint value)
        {
            value = buff[3];
            value <<= 8;
            value |= buff[2];
            value <<= 8;
            value |= buff[1];
            value <<= 8;
            value |= buff[0];
            return buff[4..];
        }

        static Span<byte> LoadUlong(Span<byte> buff, out ulong value)
        {
            buff = LoadUint(buff, out var lo);
            buff = LoadUint(buff, out var hi);
            value = hi;
            value <<= 32;
            value |= lo;
            return buff;
        }

        static void ThrowIfNotFullyConsumed(Span<byte> buff)
        {
            if (buff.IsEmpty) return;
            throw new ArgumentException($"{buff.Length} bytes not consumed", nameof(buff));
        }
    }
}
