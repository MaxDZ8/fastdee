using System.Linq;
using Newtonsoft.Json.Linq;

namespace fastdee.Stratum
{
    /// <summary>
    /// Isolates the various policies to turn magic result values into concrete instances.
    /// </summary>
    public static class ResponseParser
    {
        /// <summary>
        /// Parsing the response of "mining.subscribe". It is quite convoluted as this uses heterogeneous arrays.
        /// </summary>
        static public Response.MiningSubscribe MiningSubscribe(object? jsonLine)
        {
            if (jsonLine is not JArray arr) throw new BadParseException("mining.subscribe: result must be an array");
            if (arr[0] is not JArray first) throw new BadParseException("mining.subscribe: result[0] must be an array");
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
            var extraNonce1 = HexHelp.DecodeHex(extra1str);
            if (arr[2].Type != JTokenType.Integer) throw new BadParseException("mining.subscribe: result[2] must be an integral number");
            var nonce2sz = arr[2].Value<long>();
            if (nonce2sz != 4) throw new BadParseException("mining.subscribe: for the time being, nonce2sz must be 4");
            if (nonce2sz < 0 || nonce2sz > ushort.MaxValue) throw new BadParseException($"mining.subscribe: nonce2sz is invalid ({nonce2sz})");
            return new Response.MiningSubscribe(sessionId, extraNonce1, (ushort)nonce2sz);
        }

        internal static bool Submit(object? result)
        {
            if (null == result) throw new MissingRequiredException("mining.submit: outcome missing");
            if (result is bool real) return real;
            throw new BadParseException("mining.submit: nonce accept result must be true (false?)");
        }

        static public bool MiningAuthorize(object? result)
        {
            if (null == result) throw new MissingRequiredException("mining.authorize: outcome missing");
            if (result is bool real) return real;
            throw new BadParseException("mining.authorize: authorization must be true/false");
        }
    }
}
