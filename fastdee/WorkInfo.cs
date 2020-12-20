using System;
using System.Collections.Generic;
using System.Linq;

namespace fastdee
{
    /// <summary>
    /// Everything required to produce a block header to hash.
    /// Some comes from the initial stratum subscription, most from the mining.notify updates.
    /// </summary>
    public class WorkInfo
    {
        /// <summary>
        /// Given coinbase, generate the first merkle root. Some coins do it differently.
        /// </summary>
        public delegate Mining.Merkle FromCoinbaseFunc(byte[] coinbase);

        byte[] extraNonceOne = Array.Empty<byte>();
        ushort extraNonceTwoByteCount;

        Stratum.Notification.NewJob? currently;

        int nonce2Off;
        byte[] header = Array.Empty<byte>();

        public ReadOnlySpan<byte> Header => header;

        public void NonceSettings(byte[] extraNonceOne, ushort extraNonceTwoByteCount)
        {
            this.extraNonceOne = extraNonceOne;
            this.extraNonceTwoByteCount = extraNonceTwoByteCount;
        }

        public void NewJob(Stratum.Notification.NewJob job, IExtraNonce2Provider nonce2,
                             FromCoinbaseFunc merkleMaker)
        {
            currently = job;
            nonce2Off = job.cbHead.Length + extraNonceOne.Length;
            var coinbase = MakeCoinbaseTemplate(job.cbHead, extraNonceOne, nonce2Off, extraNonceTwoByteCount, job.cbTail);
            StampNonce2(coinbase, nonce2Off, nonce2);
            var merkle = MakeNewMerkles(merkleMaker, job.merkles, coinbase);
            var header = job.blockVer.Concat(job.prevBlock.blob).Concat(merkle);
            if (job.trie != null) header = header.Concat(job.trie); // not present in stratum v1 docs nor in M8M but sgminer has it (?)
            header = header.Concat(job.ntime).Concat(job.networkDiff);
            header = header.Concat(noncePad); // nonce to be tested
            header = header.Concat(workPadding);
            this.header = header.ToArray();
        }

        static readonly byte[] noncePad = new byte[] { 0, 0, 0, 0 };
        static readonly byte[] workPadding = new byte[] {
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
            nonce2.Consumed();
        }

        static byte[] MakeNewMerkles(FromCoinbaseFunc merkleMaker, IReadOnlyList<Mining.Merkle> merkles, byte[] coinbase)
        {
            var root = merkleMaker(coinbase);
            return PoolOps.Merkles.BlendMerkles(root, merkles);
        }
    }
}
