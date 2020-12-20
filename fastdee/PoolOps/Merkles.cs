using System;
using System.Collections.Generic;
using System.Linq;

namespace fastdee.PoolOps
{
    /// <summary>
    /// Contains the various functions used to generate the initial merkle root.
    /// I know about two for the time being.
    /// </summary>
    public static class Merkles
    {
        /// <summary>
        /// Used by most algorithms. It's a simple sha256 of the coinbase. Can't get any simpler than that.
        /// </summary>
        static public Mining.Merkle Canonical(byte[] coinbase)
        {
            var res = new Mining.Merkle();
            System.Security.Cryptography.SHA256.TryHashData(coinbase, res.blob, out var written);
            return res;
        }

        /// <summary>
        /// Some coins liked to be different and smart by abusing the original SPHLib to sorta do the hash of an hash.
        /// </summary>
        /// <param name="coinbase"></param>
        /// <returns></returns>
        static public Mining.Merkle UselesslyComplicated(byte[] coinbase)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Here for the lack of a better idea.
        /// </summary>
        static public byte[] DoubleSha(byte[] blob)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(blob);
            return System.Security.Cryptography.SHA256.HashData(hash);
        }

        /// <summary>
        /// For the time being, this is in fact always the same thing.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="jobby"></param>
        /// <returns></returns>
        public static byte[] BlendMerkles(Mining.Merkle root, IReadOnlyList<Mining.Merkle> jobby)
        {
            Span<byte> merkleSha = stackalloc byte[64];
            for (var cp = 0; cp < 32; cp++) merkleSha[cp] = root.blob[cp];
            foreach (var el in jobby)
            {
                for (var cp = 0; cp < 32; cp++) merkleSha[cp + 32] = el.blob[cp];
                BlendMerklePackedInto(merkleSha);
            }
            return merkleSha[0..32].ToArray();
        }

        static void BlendMerklePackedInto(Span<byte> io)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(io);
            System.Security.Cryptography.SHA256.TryHashData(hash, io, out var _);
        }
    }
}
