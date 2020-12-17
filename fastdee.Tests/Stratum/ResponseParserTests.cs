using Xunit;
using Newtonsoft.Json;
using fastdee.Stratum;

namespace fastdee.Tests.Stratum
{
    public class ResponseParserTests
    {
        [Theory]
        // Data I got from a mining pool.
        [InlineData(
            "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        // Same as above but with swapped subscription properties. Order does not matter.
        [InlineData(
            "[[[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"],[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"]],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        // And of course difficulty is irrelevant, it can be stripped. The notify must be there!
        [InlineData(
            "[[[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        public void ParsingGoodSubscribeReply(string dummyReply, string sessionId, byte[] extraNonce1, int extraNonce2sz)
        {
            // That's how it comes out from the parsing. I don't like it at all but that's it.
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(dummyReply);
            var response = ResponseParser.MiningSubscribe(uglee);

            Assert.Equal(sessionId, response.sessionId);
            Assert.Equal(extraNonce1, response.extraNonceOne);
            Assert.Equal(extraNonce2sz, response.extraNonceTwoByteCount);
        }

        // Examples from above, injected with anomalies which are ignored, giving still a good result.
        // Extra subscription property arrays are, in fact, ignored.
        [Theory]
        // There can be extra arrays and they are ignored. They can even be empty.
        // Not that I ever observed this.
        [InlineData(
            "[[[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"],[]],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        // There can be extra arrays and they are ignored. They can be not-a-property.
        // Not that I ever observed this.
        [InlineData(
            "[[[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"],[\"silly\"]],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        // There can be extra arrays and they are ignored. They can ever be not arrays at all!
        // Not that I ever observed this.
        [InlineData(
            "[[[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"],undefined],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        [InlineData(
            "[[[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"],null],\"0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0x08, 0x00, 0xc0, 0xff }, 4)]
        public void ParsingWeirdSubscribeReply(string dummyReply, string sessionId, byte[] extraNonce1, int extraNonce2sz)
        {
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(dummyReply);
            var response = ResponseParser.MiningSubscribe(uglee);

            Assert.Equal(sessionId, response.sessionId);
            Assert.Equal(extraNonce1, response.extraNonceOne);
            Assert.Equal(extraNonce2sz, response.extraNonceTwoByteCount);
        }

        // There must be a "mining.notify" thing with the session id.
        [Fact]
        public void SubscribeReplyMustHaveMiningNotify()
        {
            var dummyReply = "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"]],\"0800c0ff\",4]";
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(dummyReply);
            Assert.Throws<MissingRequiredException>(() => ResponseParser.MiningSubscribe(uglee));
        }

        // nonce1 can be any length, really (whatever that means later)
        [Theory]
        [InlineData(
            "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"abcd0800c0ff\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 0xab, 0xcd, 0x08, 0x00, 0xc0, 0xff }, 4)]
        [InlineData(
            "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"fe\",4]",
            "deadbeefcafebabecffd010000000000", new byte[] { 254 }, 4)]
        public void SubscribeReplyExtranonceCanHaveDifferentLengths(string dummyReply, string sessionId, byte[] extraNonce1, int extraNonce2sz)
        {
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(dummyReply);
            var response = ResponseParser.MiningSubscribe(uglee);

            Assert.Equal(sessionId, response.sessionId);
            Assert.Equal(extraNonce1, response.extraNonceOne);
            Assert.Equal(extraNonce2sz, response.extraNonceTwoByteCount);
        }

        // nonce1 can be any length, but it must be there and it cannot have 0 length.
        [Theory]
        [InlineData(
            "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],undefined,4]")]
        [InlineData(
            "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"\",4]")]
        public void SubscribeReplyExtranonceMustBeNonempty(string dummyReply)
        {
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(dummyReply);
            Assert.Throws<MissingRequiredException>(() => ResponseParser.MiningSubscribe(uglee));
        }

        [Theory]
        [InlineData(
            "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"abc\",4]")]
        public void SubscribeReplyExtranonceMustBeIntegralHex(string dummyReply)
        {
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(dummyReply);
            Assert.Throws<BadParseException>(() => ResponseParser.MiningSubscribe(uglee));
        }

        [Fact]
        public void SubscribeReplyExtra2SzMustBePositive()
        {
            var bruh = "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"0800c0ff\",-1]";
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(bruh);
            Assert.Throws<BadParseException>(() => ResponseParser.MiningSubscribe(uglee));

        }

        [Fact]
        public void SubscribeReplyExtra2SzMustBe4() // ok really, I'm taking it easy for the time being. Even though bigger nonces might be useful in the future.
        {
            var bruh = "[[[\"mining.set_difficulty\",\"deadbeefcafebabecffd010000000000\"],[\"mining.notify\",\"deadbeefcafebabecffd010000000000\"]],\"0800c0ff\",8]";
            var uglee = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(bruh);
            Assert.Throws<BadParseException>(() => ResponseParser.MiningSubscribe(uglee));

        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")] // from M8M, I haven't seen a reply of this kind in ages!
        public void ParsableAutorizationReply(string json)
        {
            var asParsed = JsonConvert.DeserializeObject<bool>(json);
            var got = ResponseParser.MiningAuthorize(asParsed);
            var bleh = JsonConvert.SerializeObject(got);
            Assert.Equal(json, bleh);
        }
    }
}
