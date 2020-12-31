using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool Empty => throw new NotImplementedException();

        public ulong ConsumedNonces => throw new NotImplementedException();

        internal void SetHeader(ShareSubmitInfo tracking, IReadOnlyList<byte> hdr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Workers request to scan an amount of nonces.
        /// </summary>
        /// <param name="nonceCount">Contiguous nonces the worker plans to test.</param>
        internal Work WannaConsume(ulong nonceCount)
        {
            throw new NotImplementedException();
        }

        internal void SetTarget(DifficultyTarget target)
        {
            throw new NotImplementedException();
        }

        internal void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
