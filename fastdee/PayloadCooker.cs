using fastdee.Devices;
using System;
using System.Linq; // .ToArray... ugh!

namespace fastdee
{
    /// <summary>
    /// <see cref="Devices.RequestedWork"/> is our "model" of work including the required information to rebuild the
    /// hash, check the nonce and submit.
    /// 
    /// This is fairly convenient for us but the devices expect 'cooked' payload in their easy format!
    /// This helper class contains the various mappings for each supported algorithm.
    /// </summary>
    class PayloadCooker
    {
        internal static byte[] CookedPayload(RequestedWork wu, WireAlgoFormat impl) => impl switch
        {
            WireAlgoFormat.Keccak => CookIntoKeccak1600(wu),
            _ => throw new NotImplementedException($"Algorithm implementation {impl} is unsupported.")
        };

        private static byte[] CookIntoKeccak1600(RequestedWork wu)
        {
            var payload = new byte[20 * 4 + 8]; // 20 uints header, 8 bytes difficulty threshold, last uint must be nonce start
            var dst = new Span<byte>(payload);
            var src = new ReadOnlySpan<byte>(wu.header.ToArray());
            for (var loop = 0; loop < 19; loop++)
            {
                dst[0] = src[3];
                dst[1] = src[2];
                dst[2] = src[1];
                dst[3] = src[0];
                dst = dst[4..];
                src = src[4..];
            }
            dst = StoreOps.StoreUint(dst, (uint)wu.nonceBase);
            StoreOps.StoreUlong(dst, wu.diffThreshold);
            return payload;
        }
    }
}
