using Xunit;
using fastdee.Stratum;

namespace fastdee.Tests
{
    public class HexHelpTests
    {
        [Fact]
        public void ConvertsDigits()
        {
            for (var loop = 0; loop < 10; loop++)
            {
                var hex = (char)('0' + loop);
                var value = HexHelp.HexValue(hex);
                Assert.Equal(loop, value);
            }
        }

        [Fact]
        public void ConvertsLowercaseHexLetters()
        {
            for (var loop = 0; loop < 6; loop++)
            {
                var hex = (char)('a' + loop);
                var value = HexHelp.HexValue(hex);
                Assert.Equal(10 + loop, value);
            }
        }

        [Fact]
        public void UppercaseHexLettersAreBad()
        {
            for (var loop = 0; loop < 6; loop++)
            {
                var hex = (char)('A' + loop);
                Assert.Throws<BadParseException>(() => HexHelp.HexValue(hex));
            }
        }

        [Fact]
        public void HexStringCannotBeEmpty()
        {
            Assert.Throws<BadParseException>(() => HexHelp.DecodeHex(""));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("01234")]
        public void HexStringsMustHaveEvenLength(string hex)
        {
            Assert.Throws<BadParseException>(() => HexHelp.DecodeHex(hex));
        }

        [Theory]
        [InlineData("00", new byte[] { 0x00 })]
        [InlineData("012345", new byte[] { 0x01, 0x23, 0x45 })]
        public void BytesComeInStringOrder(string hex, byte[] expected)
        {
            Assert.Equal(expected, HexHelp.DecodeHex(hex));
        }
    }
}
