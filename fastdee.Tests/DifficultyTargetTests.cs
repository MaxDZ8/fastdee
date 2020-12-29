using fastdee.Stratum;
using Xunit;

namespace fastdee.Tests
{
    public class DifficultyTargetTests
    {
        /// <summary>
        /// Difficulty calculations I truly observed in the wild.
        /// </summary>
        [Theory]
        [InlineData(256.0, "keccak", 256.0, new ulong[4] { 0x00, 0x00, 0x00, 0x00000000ffff0000 })]
        [InlineData(128.0, "keccak", 128.0, new ulong[4] { 0x00, 0x00, 0x00, 0x00000001fffe0000 })]
        [InlineData(64.0, "keccak", 64.0, new ulong[4] { 0x00, 0x00, 0x00, 0x00000003fffc0000 })]
        public void SupportsDifficultyCalculationTrulyObserved(double diff, string algo, double expectedDiff, ulong[] expectedTarget)
        {
            var mults = Program.ChooseTypicalDifficulties(algo);
            var uut = Program.MakeDiffTarget(algo, mults, diff);
            Assert.Equal(expectedDiff, uut.ShareDiff);
            Assert.Equal(expectedTarget[0], uut.TargA);
            Assert.Equal(expectedTarget[1], uut.TargB);
            Assert.Equal(expectedTarget[2], uut.TargC);
            Assert.Equal(expectedTarget[3], uut.TargD);
        }

        /// <summary>
        /// Made-up calculations to further test equivalency, even for stuff not making sense.
        /// </summary>
        [Theory]
        [InlineData(12.34, "keccak", 256.0, new ulong[4] { 0x00, 0x00, 0x00, 0x00000014bec7283f })]
        [InlineData(9999878, "keccak", 128.0, new ulong[4] { 0x00, 0x00, 0xd392a03710000000, 0x000000000001ad7e })]
        [InlineData(5.78844e+13, "keccak", 64.0, new ulong[4] { 0x00, 0x00, 0x04dcd59db5bbdc80, 0x00 })]
        [InlineData(1.015654e+37, "keccak", 64.0, new ulong[4] { 0x7c00000000000000, 0x00002180d57b4700, 0x00, 0x00 })]
        [InlineData(1.1516e-7, "keccak", 64.0, new ulong[4] { 0x00, 0x00, 0x00, 0x847fae2ef954c000 })]
        [InlineData(-115.245, "keccak", 64.0, new ulong[4] {
            0xffffffffffffffff, 0xffffffffffffffff, 0xffffffffffffffff, 0xffffffffffffffff
        })]
        [InlineData(-2.35464156e+5, "keccak", 64.0, new ulong[4] { 0xffffffffffffffff, 0x00, 0x00, 0x00 })]
        public void SupportsDifficultyCalculationSynthetic(double diff, string algo, double expectedDiff, ulong[] expectedTarget)
        {
            var mults = Program.ChooseTypicalDifficulties(algo);
            var uut = Program.MakeDiffTarget(algo, mults, diff);
            Assert.Equal(expectedDiff, uut.ShareDiff);
            Assert.Equal(expectedTarget[0], uut.TargA);
            Assert.Equal(expectedTarget[1], uut.TargB);
            Assert.Equal(expectedTarget[2], uut.TargC);
            Assert.Equal(expectedTarget[3], uut.TargD);
        }

        [Fact]
        public void ZeroIsBadDiff()
        {
            var algo = "keccak";
            var mults = Program.ChooseTypicalDifficulties(algo);
            Assert.Throws<System.ArgumentException>(() => Program.MakeDiffTarget(algo, mults, .0));
        }
    }
}
