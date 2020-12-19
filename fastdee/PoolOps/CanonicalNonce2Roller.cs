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
        public int ByteCount => throw new NotImplementedException();

        public ulong NativeValue => throw new NotImplementedException();

        public void Consumed()
        {
            throw new NotImplementedException();
        }

        public void CopyIntoBuffer(Span<byte> coinbaseSlice)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
