using Xunit;
using System;
using fastdee.PoolOps;

namespace fastdee.Tests.PoolOps
{
    public class CoinbaseGeneratorTests
    {
        [Theory]
        [InlineData(4u)]
        [InlineData(8u)]
        [InlineData(11u)]
        public void SupportsArbitraryNonceSizes(uint sz)
        {
            var uut = new CoinbaseGenerator(Array.Empty<byte>(), sz);
            Assert.Equal(sz, (uint)uut.Nonce2Bytes);
        }

        /// <summary>
        /// Asking for nonce2 offset before creating a template doesn't make much sense but it is supported.
        /// </summary>
        [Fact]
        public void InitialNonceOffsetIsNonce1Length()
        {
            var uut = new CoinbaseGenerator(new byte[] { 1, 2, 3, 4, 5 }, 4);
            Assert.Equal(5, uut.Nonce2Off);
        }

        /// <summary>
        /// Generating a coinbase template updates the nonce2 offset according to extranonce1 and coinbaseFirst
        /// </summary>
        [Fact]
        public void Nonce2OffsetIsLengthSum()
        {
            var uut = new CoinbaseGenerator(new byte[] { 1, 2, 3, 4, 5 }, 4);
            uut.MakeCoinbaseTemplate(new byte[] { 6, 7, 8 }, new byte[] { 254, 255 });
            Assert.Equal(8, uut.Nonce2Off);
        }

        /// <summary>
        /// Coinbase is concatenation of: coinbase_first, extranonce1, extranonce2, coinbase_final.
        /// The generated template has 0 at the offset.
        /// </summary>
        [Fact]
        public void CoinbaseIsEssentiallyConcatenation()
        {
            var expected = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 254, 255 };
            var uut = new CoinbaseGenerator(new byte[] { 6, 7, 8 }, 4);
            var template = uut.MakeCoinbaseTemplate(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 254, 255 });
            Assert.Equal(expected, template);
        }
    }
}
