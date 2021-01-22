using Xunit;
using fastdee.Devices.Udp;
using System.Net;
using System.Collections.Generic;

namespace fastdee.Tests.Devices.Udp
{
    /// <summary>
    /// Is binary format ok? This time high level to binary blob. Kinda more complicated.
    /// </summary>
    public class ReplyMakerTests
    {
        [Theory]
        [InlineData("12.34.255.0", new byte[] { 12, 34, 255, 0 })]         // note the IP value is really irrelevant for it.
        [InlineData("0.0.0.0", new byte[] { 0, 0, 0, 0 })]                 // and you can feed really bad things
        [InlineData("255.255.255.255", new byte[] { 255, 255, 255, 255 })] // even those not making sense, no questions asked.
        // TODO: test ipv6? You could parse it and being bigger it will not produce any reasonable message.
        // IPv6 clients could ideally use ipv4 orchestrator but whatever.
        public void HelloReplyIsAddress(string serverAddr, byte[] addrBytes)
        {
            var ipaddr = IPAddress.Parse(serverAddr);
            var modelCommon = new byte[] { 0, 1, 2, 3 }; // the arguments are irrelevant as well for the time being.
            var deviceSpecific = new byte[] { 123, 101, 202 };
            var magic = new ReplyMaker();
            var reply = magic.Welcome(ipaddr, modelCommon, deviceSpecific);
            // For the time being, the reply is always the same. Outgoing packet kind, flags to zero, IP address of the server.
            var easy = new List<byte>() { (byte)OutgoingKind.ServerAddress, 0 };
            easy.AddRange(addrBytes);
            Assert.Equal(easy, reply); // the reply is merely for bytes being the ipv4 address.
        }
    }
}
