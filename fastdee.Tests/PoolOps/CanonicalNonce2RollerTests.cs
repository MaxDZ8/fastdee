using System;
using Xunit;
using fastdee.PoolOps;

namespace fastdee.Tests.PoolOps
{
    public class CanonicalNonce2RollerTests
    {
        [Fact]
        public void NonceSizeIs4()
        {
            var uut = new CanonicalNonce2Roller();
            Assert.Equal(4, uut.ByteCount);
        }

        [Fact]
        public void InitialNonceIs0()
        {
            var uut = new CanonicalNonce2Roller();
            Assert.Equal((ulong)0, uut.NativeValue);
        }

        [Fact]
        public void GettingValueIsReadOnly()
        {
            var uut = new CanonicalNonce2Roller();
            var initially = uut.NativeValue;
            var later = uut.NativeValue;
            var lastly = uut.NativeValue;
            Assert.Equal(initially, later);
            Assert.Equal(initially, lastly);
        }

        [Fact]
        public void StampingValueIsReadOnly()
        {
            var uut = new CanonicalNonce2Roller();
            var buffer = new byte[4];
            var initially = uut.NativeValue;
            uut.CopyIntoBuffer(buffer);
            Assert.Equal(initially, uut.NativeValue);
        }

        [Fact]
        public void ConsumeIncrementsByOne()
        {
            var uut = new CanonicalNonce2Roller();
            uut.Consumed();
            Assert.Equal((ulong)1, uut.NativeValue);
        }

        [Fact]
        public void ResetGoesBackToZero()
        {
            var uut = new CanonicalNonce2Roller();
            for (var loop = 0; loop < 42; loop++) uut.Consumed();
            uut.Reset();
            Assert.Equal((ulong)0, uut.NativeValue);
        }

        [Fact]
        public void StampedValueIsLittleEndian()
        {
            var uut = new CanonicalNonce2Roller();
            var buffer = new byte[4];
            for (var loop = 0; loop < 0x12345678; loop++) uut.Consumed();
            uut.CopyIntoBuffer(buffer);
            Assert.Equal(new byte[] { 0x78, 0x56, 0x34, 0x12 }, buffer);
        }
    }
}
