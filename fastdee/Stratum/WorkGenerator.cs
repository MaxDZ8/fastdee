using System;
using System.Collections.Generic;
using System.Linq;

namespace fastdee.Stratum
{
    /// <summary>
    /// Assemble header and difficulty in a "work" to be hashed.
    /// Keep a nonce scanning count.
    /// 
    /// Workers can request to reserve a scan range.
    /// If their requests cannot be satisfied, it's an error. Something else will have to roll nonce2
    /// and give me a new header.
    /// </summary>
    class WorkGenerator
    {
        class Both
        {
            internal ShareSubmitInfo info;
            internal IReadOnlyList<byte> header;

            internal Both(ShareSubmitInfo info, IReadOnlyList<byte> header)
            {
                this.info = info;
                this.header = header;
            }
        }
        Both? both; // capture pair non-null
        DifficultyTarget? target;

        public bool Empty => null == target || null == both;

        public uint ConsumedNonces { get; private set; }

        public void NextNonce(uint value) { ConsumedNonces = value; }

        internal void SetHeader(ShareSubmitInfo tracking, IReadOnlyList<byte> hdr)
        {
            bool changed;
            if (null == both)
            {
                both = new Both(tracking, hdr);
                changed = true;
            }
            else
            {
                changed = hdr.SequenceEqual(both.header) == false;
                if (changed) both.header = hdr;
                both.info = tracking;
            }
            if (changed) ConsumedNonces = 0;
        }

        /// <summary>
        /// Workers request to scan an amount of nonces.
        /// </summary>
        /// <param name="nonceCount">Contiguous nonces the worker plans to test.</param>
        internal Work WannaConsume(uint nonceCount)
        {
            if (null == target || null == both)
            {
                // Can't use Empty here, the static analyzer won't guess the non-nullability
                throw new InvalidOperationException("won't produce work before being fed");
            }
            if (nonceCount == 0) throw new ArgumentException("you must consume at least a nonce", nameof(nonceCount));
            if (rem < nonceCount) throw new ArgumentException("too many nonces requested", nameof(nonceCount));
            var rem = uint.MaxValue - ConsumedNonces; // better to stick on 32 bit scan ranges and roll n2.
            var res = new Work(target, both.header, both.info, ConsumedNonces);
            ConsumedNonces += nonceCount;
            return res;
        }

        internal void SetTarget(DifficultyTarget target) => this.target = target;

        internal void Stop()
        {
            // Note: target is kept.
            both = null;
        }
    }
}
