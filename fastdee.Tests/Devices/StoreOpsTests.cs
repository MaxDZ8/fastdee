using fastdee.Devices;
using Xunit;

namespace fastdee.Tests.Devices
{
    public class StoreOpsTests
    {
        [Fact]
        public void CanStoreByte()
        {
            var blob = new byte[1];
            var span = StoreOps.StoreByte(blob, 5);
            Assert.True(span.IsEmpty);
            Assert.Equal(5, blob[0]);
        }

        [Fact]
        public void UshortStoredBigEndian()
        {
            var blob = new byte[2];
            var span = StoreOps.StoreUshort(blob, 0xABCD);
            Assert.True(span.IsEmpty);
            Assert.Equal(0xCD, blob[0]);
            Assert.Equal(0xAB, blob[1]);
        }

        [Fact]
        public void UintStoredBigEndian()
        {
            var blob = new byte[4];
            var span = StoreOps.StoreUint(blob, 0xABCDEF01);
            Assert.True(span.IsEmpty);
            Assert.Equal(0x01, blob[0]);
            Assert.Equal(0xEF, blob[1]);
            Assert.Equal(0xCD, blob[2]);
            Assert.Equal(0xAB, blob[3]);
        }

        [Fact]
        public void UlongStoredBigEndian()
        {
            var blob = new byte[8];
            var span = StoreOps.StoreUlong(blob, 0xABCDEF01_23456789);
            Assert.True(span.IsEmpty);
            Assert.Equal(0x89, blob[0]);
            Assert.Equal(0x67, blob[1]);
            Assert.Equal(0x45, blob[2]);
            Assert.Equal(0x23, blob[3]);
            Assert.Equal(0x01, blob[4]);
            Assert.Equal(0xEF, blob[5]);
            Assert.Equal(0xCD, blob[6]);
            Assert.Equal(0xAB, blob[7]);
        }
    }
}
