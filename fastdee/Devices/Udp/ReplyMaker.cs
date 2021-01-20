using System;
using System.Net;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// The <see cref="EventInstantiator"/> mangles the data you received from the socket and, elevates it to real structures.
    /// The <see cref="ReplyMaker"/> is in some way the opposite, but slightly scaled up.
    /// It takes care of both building the reply and serializing it to a blob you send over the wire.
    /// </summary>
    class ReplyMaker
    {
        readonly IPAddress ipaddr;
        readonly byte[] addrBlob;

        public ReplyMaker(IPAddress ipaddr)
        {
            this.ipaddr = ipaddr; // keep it floating around mostly for debug reasons.
            addrBlob = ipaddr.GetAddressBytes();
        }

        /// <summary>
        /// Construct the reply to <see cref="TurnOnArgs{A}"/>. In line of principle you could not-reply to
        /// some devices as they could be mangled by somebody else but... for the time being it's just a matter
        /// of giving back the server address to perform unicast packet transmissions.
        /// </summary>
        /// <param name="identificator">Currently unused.</param>
        /// <param name="deviceSpecific">Currently unused.</param>
        /// <returns>Blob to send over the wire. I won't touch it anymore.</returns>
        internal byte[]? Reply(byte[] identificator, byte[] deviceSpecific)
        {
            var blob = new byte[1 + 1 + addrBlob.Length];
            var store = StoreKind(blob, OutgoingKind.ServerAddress);
            store = StoreByte(store, 0); // this is really a bitflag of features. For the time being it must be zero so no big deal
            store = StoreByteArray(store, addrBlob);
            ThrowIfNotFullyConsumed(store);
            return blob;
        }


        /// <summary>
        /// Convenience.
        /// </summary>
        static Span<byte> StoreKind(Span<byte> buff, OutgoingKind kind) => StoreByte(buff, (byte)kind);

        static Span<byte> StoreByte(Span<byte> buff, byte value)
        {
            buff[0] = value;
            return buff[1..];
        }

        static Span<byte> StoreByteArray(Span<byte> buff, ReadOnlySpan<byte> value)
        {
            for (var cp = 0; cp < value.Length; cp++) buff[cp] = value[cp];
            return buff[value.Length..];
        }

        static void ThrowIfNotFullyConsumed(Span<byte> buff)
        {
            if (buff.IsEmpty) return;
            throw new ArgumentException($"{buff.Length} bytes not consumed", nameof(buff));
        }
    }
}
