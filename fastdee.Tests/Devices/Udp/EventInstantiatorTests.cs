using Xunit;
using System.Linq;
using fastdee.Devices.Udp;
using System.Net;

namespace fastdee.Tests.Devices.Udp
{
    /// <summary>
    /// Basically test binary format is ok.
    /// </summary>
    public class EventInstantiatorTests
    {
        [Fact]
        public void HelloLoadsCorrectly()
        {
            const string devuniq = "mdz-hwswmc-udp-bare-zynq"; // mdz hybrid hardware-software-mining-contraption
            var binuniq = AsciiBlob(devuniq);
            var devspec = new byte[] { // this is basically arbitrary, the parsing is defined by devuniq (mostly), but can further refine
                0xFA, // device-specific-format: next parse is versions
                0, 1, 2, 3, // hardware revision
                4, 5, 6, 7 // firmware/software revision version
            };
            var hdr = new byte[] {
                (byte)binuniq.Length,
                (byte)devspec.Length
            };
            var fake = hdr.Concat(binuniq).Concat(devspec).ToArray();
            var pretending = Dummy;
            var magic = EventInstantiator.Hello(pretending, fake);
            Assert.Same(pretending, magic.originator);
            Assert.Equal(binuniq, magic.identificator);
            Assert.Equal(devspec, magic.deviceSpecific);
        }

        [Fact]
        public void HelloMustConsumeAll()
        {
            const string devuniq = "mdz-hwswmc-udp-bare-zynq"; // mdz hybrid hardware-software-mining-contraption
            var binuniq = AsciiBlob(devuniq);
            var devspec = new byte[] { // this is basically arbitrary, the parsing is defined by devuniq (mostly), but can further refine
                0xFA, // device-specific-format: next parse is versions
                0, 1, 2, 3, // hardware revision
                4, 5, 6, 7 // firmware/software revision version
            };
            var hdr = new byte[] {
                (byte)binuniq.Length,
                (byte)devspec.Length
            };
            var fake = hdr.Concat(binuniq).Concat(devspec).Concat(new byte[] { 1 }).ToArray();
            var pretending = Dummy;
            Assert.Throws<System.ArgumentException>(() => EventInstantiator.Hello(pretending, fake));
        }

        [Fact]
        public void GimmeWorkLoadsCorrectly()
        {
            var bin = new byte[] {
                (byte)fastdee.Devices.WireAlgoFormat.Keccak, // algo lo
                0, // algo hi - big endian
                0x12, 0x34, 0x56, 0x78 // nonce count, big endian
            };
            var pretending = Dummy;
            var magic = EventInstantiator.GimmeWork(pretending, bin);
            Assert.Same(pretending, magic.originator);
            Assert.Equal(fastdee.Devices.WireAlgoFormat.Keccak, magic.algoFormat);
            Assert.Equal(0x78563412ul, magic.scanCount);
        }

        [Fact]
        public void GimmeWorkMustConsumeAll()
        {
            var bin = new byte[] {
                (byte)fastdee.Devices.WireAlgoFormat.Keccak, // algo lo
                0, // algo hi - big endian
                0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF1, // nonce count, big endian
                0xFF
            };
            var pretending = Dummy;
            Assert.Throws<System.ArgumentException>(() => EventInstantiator.GimmeWork(pretending, bin));
        }

        [Fact]
        public void FoundNonceLoadsCorrectly()
        {
            var bin = new byte[] {
                0x01, 0x23, 0x45, 0x67, // original workid, big endian
                0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, // increment wrt original base-nonce
                2, // this hash produces 2 uints (weird, but it's just a test) - max is 255 uints, unlikely but I guess 192 might be a thing.
                0xA0, 0xB1, 0xC2, 0xD3,
                0x4E, 0x5F, 0x60, 0x71 // the format of the hash is a function of the algorithm header format
            };
            var magic = EventInstantiator.FoundNonce( bin);
            Assert.Equal(0x67452301u, magic.workid);
            Assert.Equal(0x7766554433221100ul, magic.increment);
        }

        [Fact]
        public void FoundNonceMustConsumeAll()
        {
            var bin = new byte[] {
                0x01, 0x23, 0x45, 0x67,
                0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
                1, // one extra uint given
                0xA0, 0xB1, 0xC2, 0xD3,
                0x4E, 0x5F, 0x60, 0x71
            };
            Assert.Throws<System.ArgumentException>(() => EventInstantiator.FoundNonce(bin));
        }

        static byte[] AsciiBlob(string ascii) => System.Text.Encoding.ASCII.GetBytes(ascii);
        static IPEndPoint Dummy => new IPEndPoint(IPAddress.Any, 12345);
    }
}
