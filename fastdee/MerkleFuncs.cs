namespace fastdee
{
    /// <summary>
    /// Merkle tree generation also shows some variation across different algorithms.
    /// </summary>
    public class MerkleFuncs
    {
        /// <summary>
        /// Given coinbase, generate the first merkle root.
        /// This is the main issue.
        /// </summary>
        public delegate Mining.Merkle FromCoinbaseFunc(byte[] coinbase);

        public readonly FromCoinbaseFunc makeRootFromCoinbase;

        public MerkleFuncs(FromCoinbaseFunc initialCrunch)
        {
            makeRootFromCoinbase = initialCrunch;
        }

        public byte[] BlendMerkles(Mining.MerkleRoot root, Mining.MerkleRoot[] jobby)
        {
            throw new System.NotImplementedException();
        }
    }
}
