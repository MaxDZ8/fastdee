using System.Linq;
using Newtonsoft.Json.Linq;

namespace fastdee.Stratum
{
    /// <summary>
    /// Isolates the various policies to turn magic result values into concrete instances.
    /// </summary>
    public class ResponseParser
    {
        /// <summary>
        /// Parsing the response of "mining.subscribe". It is quite convoluted as this uses heterogeneous arrays.
        /// </summary>
        public Response.MiningSubscribe MiningSubscribe(object? jsonLine)
        {
            var arr = jsonLine as JArray;
            if (null == arr) throw new BadParseException("mining.subscribe: result must be an array");
            var first = arr[0] as JArray;
            if (null == first) throw new BadParseException("mining.subscribe: result[0] must be an array");
            var parts = from el in first
                        where el.Type == JTokenType.Array
                        select el as JArray;
            var sessionId = (from el in parts
                             where el.Count == 2
                             where el[0].Type == JTokenType.String && el[0].Value<string>() == "mining.notify"
                             where el[1].Type == JTokenType.String
                             select el[1].Value<string>()).FirstOrDefault();
            if (null == sessionId) throw new MissingRequiredException("mining.subscribe: couldn't find any sessionid mining.notify pair");
            if (arr[1].Type != JTokenType.String)
            {
                var careful = arr[1].Type;
                if (careful == JTokenType.Undefined || careful == JTokenType.Null) throw new MissingRequiredException("mining.subscribe: missing extraNonce1");
                throw new BadParseException("mining.subscribe: extraNonce1 must be a string");
            }
            var extra1str = arr[1].Value<string>().Trim();
            if (extra1str.Length == 0) throw new MissingRequiredException("mining.subscribe: extraNonce1 is an empty string");
            var extraNonce1 = DecodeHex(extra1str);
            if (arr[2].Type != JTokenType.Integer) throw new BadParseException("mining.subscribe: result[2] must be an integral number");
            var nonce2sz = arr[2].Value<long>();
            if (nonce2sz != 4) throw new BadParseException("mining.subscribe: for the time being, nonce2sz must be 4");
            if (nonce2sz < 0 || nonce2sz > ushort.MaxValue) throw new BadParseException($"mining.subscribe: nonce2sz is invalid ({nonce2sz})");
            return new Response.MiningSubscribe(sessionId, extraNonce1, (ushort)nonce2sz);
        }

        public bool MiningAuthorize(object? result)
        {
            throw new System.NotImplementedException();
        }

        static byte[] DecodeHex(string hex)
        {
            if (hex.Length == 0) throw new BadParseException("Hexadecimal strings cannot be empty");
            if (hex.Length % 2 != 0) throw new BadParseException("Hexadecimal strings must have even digit count");
            hex = hex.ToLowerInvariant();
            var blobby = new byte[hex.Length / 2];
            for (var loop = 0; loop < hex.Length; loop += 2) blobby[loop / 2] = HexValue(hex[loop + 0], hex[loop + 1]);
            return blobby;
        }

        static byte HexValue(char hi, char lo) => (byte)((HexValue(hi) << 4) | HexValue(lo));

        static byte HexValue(char digit)
        {
            if (digit >= '0' && digit <= '9') return (byte)(     digit - '0');
            if (digit >= 'a' && digit <= 'f') return (byte)(10 + digit - 'a');
            throw new BadParseException("Invalid character in hex digit");
        }
    }
}
