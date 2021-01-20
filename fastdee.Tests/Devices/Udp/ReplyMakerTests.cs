using Xunit;
using fastdee.Devices.Udp;
using System.Net;

namespace fastdee.Tests.Devices.Udp
{
    /// <summary>
    /// Is binary format ok? This time high level to binary blob. Kinda more complicated.
    /// </summary>
    public class ReplyMakerTests
    {
        [Theory]
        [InlineData("12.34.255.0", new byte[] { 0, 255, 34, 12 })]         // note the IP value is really irrelevant for it.
        [InlineData("0.0.0.0", new byte[] { 0, 0, 0, 0 })]                 // and you can feed really bad things
        [InlineData("255.255.255.255", new byte[] { 255, 255, 255, 255 })] // even those not making sense, no questions asked.
        public void HelloReplyIsAddress(string serverAddr, byte[] expected)
        {
            var ipaddr = IPAddress.Parse(serverAddr);
            var modelCommon = new byte[] { 0, 1, 2, 3 }; // the arguments are irrelevant as well for the time being.
            var deviceSpecific = new byte[] { 123, 101, 202 };
            var magic = new ReplyMaker(ipaddr);
            var reply = magic.Reply(modelCommon, deviceSpecific);
            Assert.Same(expected, reply); // the reply is merely for bytes being the ipv4 address.
        }
    }
}
