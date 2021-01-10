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
            throw new System.NotImplementedException();
        }

        [Fact]
        public void GimmeWorkMustConsumeAll()
        {
            throw new System.NotImplementedException();
        }

        [Fact]
        public void FoundNonceLoadsCorrectly()
        {
            throw new System.NotImplementedException();
        }

        [Fact]
        public void FoundNonceMustConsumeAll()
        {
            throw new System.NotImplementedException();
        }

        static byte[] AsciiBlob(string ascii) => System.Text.Encoding.ASCII.GetBytes(ascii);
        static IPEndPoint Dummy => new IPEndPoint(IPAddress.Any, 12345);
    }
}
