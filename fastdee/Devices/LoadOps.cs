using fastdee.Devices.Udp;
using System;
using System.Collections.Generic;

namespace fastdee.Devices
{
    static class LoadOps
    {
        internal static ReadOnlySpan<byte> LoadByte(ReadOnlySpan<byte> buff, out byte value)
        {
            value = buff[0];
            return buff[1..];
        }

        internal static ReadOnlySpan<byte> LoadByteArr(ReadOnlySpan<byte> buff, Span<byte> value)
        {
            for (var cp = 0; cp < value.Length; cp++) value[cp] = buff[cp];
            return buff[value.Length..];
        }

        internal static ReadOnlySpan<byte> LoadByteArr(ReadOnlySpan<byte> buff, int elements, out byte[] value)
        {
            value = 0 == elements ? Array.Empty<byte>() : new byte[elements];
            return LoadByteArr(buff, value);
        }

        internal static ReadOnlySpan<byte> LoadByteArr(ReadOnlySpan<byte> buff, byte[] value)
        {
            for (var cp = 0; cp < value.Length; cp++) value[cp] = buff[cp];
            return buff[value.Length..];
        }

        internal static ReadOnlySpan<byte> LoadUshort(ReadOnlySpan<byte> buff, out ushort value)
        {
            value = buff[1];
            value <<= 8;
            value |= buff[0];
            return buff[2..];
        }

        internal static ReadOnlySpan<byte> LoadUint(ReadOnlySpan<byte> buff, out uint value)
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

        internal static ReadOnlySpan<byte> LoadUlong(ReadOnlySpan<byte> buff, out ulong value)
        {
            buff = LoadUint(buff, out var lo);
            buff = LoadUint(buff, out var hi);
            value = hi;
            value <<= 32;
            value |= lo;
            return buff;
        }
    }
}
