﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace fastdee.Stratum
{
    /// <summary>
    /// Everything required to produce a block header to hash.
    /// Some comes from the initial stratum subscription, most from the mining.notify updates.
    /// </summary>
    public class HeaderGenerator
    {
        /// <summary>
        /// Given coinbase, generate the first merkle root. Some coins do it differently.
        /// </summary>
        public delegate Mining.Merkle FromCoinbaseFunc(byte[] coinbase);

        byte[] extraNonceOne = Array.Empty<byte>();
        readonly PoolOps.CanonicalNonce2Roller nonce2 = new PoolOps.CanonicalNonce2Roller();

        Notification.NewJob? currently;

        int nonce2Off;
        byte[] header = Array.Empty<byte>();

        readonly FromCoinbaseFunc initialMerkle;

        /// <summary>
        /// You can always pull an header out of me and it's always non-null and maybe even non-empty...
        /// But it's only useful if <see cref="Significative"/> evaluates to true.
        /// </summary>
        public ReadOnlySpan<byte> Header => header;
        public bool Significative => currently != null;

        public HeaderGenerator(FromCoinbaseFunc initialMerkle)
        {
            this.initialMerkle = initialMerkle;
        }

        public void NonceSettings(byte[] extraNonceOne, ushort extraNonceTwoByteCount)
        {
            this.extraNonceOne = extraNonceOne;
            if (4 != extraNonceTwoByteCount) throw new NotImplementedException("only supported nonce2 size is 4");
        }

        public void NewJob(Notification.NewJob job)
        {
            currently = job;
            nonce2Off = job.cbHead.Length + extraNonceOne.Length;
            var coinbase = MakeCoinbaseTemplate(job.cbHead, extraNonceOne, nonce2Off, nonce2.ByteCount, job.cbTail);
            StampNonce2(coinbase, nonce2Off, nonce2);
            var merkle = MakeNewMerkles(initialMerkle, job.merkles, coinbase);
            SwapUintBytes(merkle); // that's from stratum documentation
            var header = job.blockVer.Concat(job.prevBlock.blob).Concat(merkle);
            if (job.trie != null) header = header.Concat(job.trie); // not present in stratum v1 docs nor in M8M but sgminer has it (?)
            header = header.Concat(job.ntime).Concat(job.networkDiff);
            header = header.Concat(noncePad); // nonce to be tested
            header = header.Concat(workPadding);
            this.header = header.ToArray();

            // But legacy miners have a quirk. They just zero out the whole thing (at least in the code I have checked).
            this.header[^45] = 0;
            this.header[^4] = 0;
            this.header[^3] = 0;
        }

        static readonly byte[] noncePad = new byte[] { 0, 0, 0, 0 };
        static readonly byte[] workPadding = new byte[] { // work padding from official stratum documentation
            0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00,    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,    0x00, 0x00, 0x00, 0x00, 0x80, 0x02, 0x00, 0x00
        };

        /// <summary>
        /// coinbase = coinbase_first, extranonce1, extranonce2, coinbase_final
        /// </summary>
        static byte[] MakeCoinbaseTemplate(byte[] cbHead, byte[] extraNonce1, int n2off, int n2sz, byte[] cbTail)
        {
            var taking = n2off + n2sz + cbTail.Length;
            var res = new byte[taking]; // yeah you can do magic with Concat and stuff but cmon
            Array.Copy(cbHead, 0, res, 0, cbHead.Length);
            Array.Copy(extraNonce1, 0, res, cbHead.Length, extraNonce1.Length);
            Array.Copy(cbTail, 0, res, n2off + n2sz, cbTail.Length);
            return res;
        }

        static void StampNonce2(byte[] coinbase, int nonce2Off, IExtraNonce2Provider nonce2)
        {
            nonce2.CopyIntoBuffer(new Span<byte>(coinbase, nonce2Off, nonce2.ByteCount));
        }

        static byte[] MakeNewMerkles(FromCoinbaseFunc merkleMaker, IReadOnlyList<Mining.Merkle> merkles, byte[] coinbase)
        {
            var root = merkleMaker(coinbase);
            return PoolOps.Merkles.BlendMerkles(root, merkles);
        }

        static void SwapUintBytes(Span<byte> blob)
        {
            while (blob.Length >= 4)
            {
                var ta = blob[0];
                var tb = blob[1];
                blob[0] = blob[3];
                blob[1] = blob[2];
                blob[2] = tb;
                blob[3] = ta;
                blob = blob[4..];
            }
        }

        internal void Stop()
        {
            currently = null;
        }

        internal void NextNonce(ulong n) => nonce2.NextNonce(n);

        internal byte[] CopyNonce2()
        {
            var buff = new byte[nonce2.ByteCount];
            nonce2.CopyIntoBuffer(buff);
            return buff;
        }
    }
}
