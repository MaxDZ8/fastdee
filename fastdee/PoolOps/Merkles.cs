using System;

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
    }
}
