using System;
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
        static public Mining.MerkleRoot Canonical(byte[] coinbase)
        {
            var res = new Mining.MerkleRoot();
            System.Security.Cryptography.SHA256.TryHashData(coinbase, res.blob, out var written);
            return res;
        }

        /// <summary>
        /// Some coins liked to be different and smart by abusing the original SPHLib to sorta do the hash of an hash.
        /// </summary>
        /// <param name="coinbase"></param>
        /// <returns></returns>
        static public Mining.MerkleRoot UselesslyComplicated(byte[] coinbase)
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
        static public Mining.MerkleRoot BlendMerkle(byte[] root, byte[] merkle)
        {
            var longer = root.Concat(merkle).ToArray();
            var hash = System.Security.Cryptography.SHA256.HashData(longer);
            var res = new Mining.MerkleRoot();
            System.Security.Cryptography.SHA256.TryHashData(hash, res.blob, out var _);
            return res;
        }
    }
}
