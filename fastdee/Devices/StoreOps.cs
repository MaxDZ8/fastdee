using fastdee.Devices.Udp;
using System;
using System.Collections.Generic;

namespace fastdee.Devices
{
    /// <summary>
    /// There has been a point this was for serializing blobs to the devices but it comes handy to work-unit
    /// adjustment (cooking to algorithm implementation payload) so it got its own class.
    /// </summary>
    static class StoreOps
    {
        /// <summary>
        /// Convenience.
        /// </summary>
        internal static Span<byte> StoreKind(Span<byte> buff, OutgoingKind kind) => StoreByte(buff, (byte)kind);

        internal static Span<byte> StoreByte(Span<byte> buff, byte value)
        {
            buff[0] = value;
            return buff[1..];
        }

        internal static Span<byte> StoreByteArray(Span<byte> buff, ReadOnlySpan<byte> value)
        {
            for (var cp = 0; cp < value.Length; cp++) buff[cp] = value[cp];
            return buff[value.Length..];
        }

        // TODO: maybe just IEnumerable?
        internal static Span<byte> StoreByteList(Span<byte> buff, IReadOnlyList<byte> value)
        {
            for (var cp = 0; cp < value.Count; cp++) buff[cp] = value[cp];
            return buff[value.Count..];
        }

        internal static Span<byte> StoreUshort(Span<byte> buff, ushort value)
        {
            buff[0] = (byte)(value);
            buff[1] = (byte)(value >> 8);;
            return buff[2..];
        }

        internal static Span<byte> StoreUint(Span<byte> buff, uint value)
        {
            buff[0] = (byte)(value);
            buff[1] = (byte)(value >> 8);
            buff[2] = (byte)(value >> 16);
            buff[3] = (byte)(value >> 24);
            return buff[4..];
        }

        internal static Span<byte> StoreUlong(Span<byte> buff, ulong value)
        {
            var hi = (uint)(value >> 32);
            var lo = (uint)(value);
            buff = StoreUint(buff, lo);
            buff = StoreUint(buff, hi);
            return buff;
        }
    }
}
