using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.PoolOps
{
    /// <summary>
    /// Usual, canonical nonce rolling.
    /// Increments a 32-bit number, that's it!
    /// </summary>
    public class CanonicalNonce2Roller : IExtraNonce2Provider
    {
        uint nonce2;

        public int ByteCount => 4;

        public ulong NativeValue => nonce2;

        public void Consumed() => nonce2++;

        public void CopyIntoBuffer(Span<byte> slice)
        {
            slice[3] = (byte)(nonce2 >> 24);
            slice[2] = (byte)(nonce2 >> 16);
            slice[1] = (byte)(nonce2 >>  8);
            slice[0] = (byte)nonce2;
        }

        public void Reset() => nonce2 = 0;

        public void NextNonce(ulong nonce2)
        {
            if (nonce2 > uint.MaxValue) throw new ArgumentOutOfRangeException(nameof(nonce2));
            this.nonce2 = (uint)nonce2;
        }
    }
}
