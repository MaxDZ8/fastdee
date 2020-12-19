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
            Nonce2Off = ExtraNonce1Bytes;
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
        public byte[] MakeCoinbaseTemplate(byte[] cbHead, byte[] cbTail)
        {
            Nonce2Off = cbHead.Length + extraNonce1.Length;
            var taking = Nonce2Off + nonce2sz + cbTail.Length;
            var res = new byte[taking]; // yeah you can do magic with Concat and stuff but cmon
            Array.Copy(cbHead, 0, res, 0, cbHead.Length);
            Array.Copy(extraNonce1, 0, res, cbHead.Length, extraNonce1.Length);
            Array.Copy(cbTail, 0, res, Nonce2Off + nonce2sz, cbTail.Length);
            return res;
        }
    }
}
