using System;

namespace fastdee.PoolOps
{
    /// <summary>
    /// Helper to keep a couple variables together. Not exceedingly useful.
    /// </summary>
    public class CoinbaseGenerator
    {
        readonly byte[] extraNonce1;
        readonly uint nonce2sz;

        /// <summary>
        /// Save a few variables from <see cref="Stratum.Response.MiningSubscribe"/>.
        /// </summary>
        public CoinbaseGenerator(byte[] extraNonce1, uint nonce2sz)
        {
            this.extraNonce1 = extraNonce1;
            this.nonce2sz = nonce2sz;
        }

        public int ExtraNonce1Bytes => extraNonce1.Length;
        public int Nonce2Bytes => (int)nonce2sz;
        public int Nonce2Off { get; private set; }

        /// <summary>
        /// coinbase = coinbase_first, extranonce1, extranonce2, coinbase_final
        /// </summary>
        /// <remarks>
        /// Also keeps track of coinbase1 length so I can give you the offset right away.
        /// </remarks>
        public byte[] MakeCoinbaseTemplate(byte[] cbfirst, byte[] cbfinal)
        {
            throw new NotImplementedException();
        }
    }
}
