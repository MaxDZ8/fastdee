using fastdee.Devices;
using Xunit;

namespace fastdee.Tests.Devices
{
    public class LoadOpsTests
    {
        [Fact]
        public void CanLoadByte()
        {
            byte expect = 5;
            var blob = new byte[1] { expect };
            var span = LoadOps.LoadByte(blob, out var loaded);
            Assert.True(span.IsEmpty);
            Assert.Equal(expect, loaded);
        }

        [Fact]
        public void UshortLoadedFromBigEndian()
        {
            ushort expect = 0xABCD;
            var blob = new byte[2] { 0xCD, 0xAB };
            var span = LoadOps.LoadUshort(blob, out var loaded);
            Assert.True(span.IsEmpty);
            Assert.Equal(expect, loaded);
        }

        [Fact]
        public void UintLoadedFromBigEndian()
        {
            uint expect = 0xABCDEF01;
            var blob = new byte[4] { 0x01, 0xEF, 0xCD, 0xAB };
            var span = LoadOps.LoadUint(blob, out var loaded);
            Assert.True(span.IsEmpty);
            Assert.Equal(expect, loaded);
        }

        [Fact]
        public void UlongLoadedFromBigEndian()
        {
            ulong expect = 0xABCDEF01_23456789;
            var blob = new byte[8] { 0x89, 0x67, 0x45, 0x23, 0x01, 0xEF, 0xCD, 0xAB };
            var span = LoadOps.LoadUlong(blob, out var loaded);
            Assert.True(span.IsEmpty);
            Assert.Equal(expect, loaded);
        }
    }
}
