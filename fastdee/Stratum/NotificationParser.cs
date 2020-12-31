using Newtonsoft.Json.Linq;
using System.Linq;

namespace fastdee.Stratum
{
    public class NotificationParser
    {
        static public Notification.NewJob MiningNotify(object? evargs)
        {
            if (null == evargs) throw new MissingRequiredException("mining.notify: no payload given");
            if (evargs is not JArray concrete) throw new MissingRequiredException("mining.notify: payload must be an array");
            if (concrete.Count != 9 && concrete.Count != 10) throw new BadParseException("mining.notify: element count mismatch, won't be able to parse");

            var getTrie = concrete.Count == 10;
            byte[]? trie = null;
            var index = 0;
            var jobid = concrete[index++].Value<string>();
            var prevHashHexstr = concrete[index++].Value<string>();
            if (getTrie)
            {
                var triestr = concrete[index++].Value<string>();
                trie = HexHelp.DecodeHex(triestr);
            }
            var coinbaseFirst = concrete[index++].Value<string>();
            var coinBaseSecond = concrete[index++].Value<string>();
            var merkles = concrete[index++] as JArray ?? throw new BadParseException("mining.notify: merkles must be an array");
            var version = HexHelp.DecodeHex(concrete[index++].Value<string>());
            var nbits = HexHelp.DecodeHex(concrete[index++].Value<string>());
            var ntime = HexHelp.DecodeHex(concrete[index++].Value<string>());
            var flush = concrete[index++].Value<bool>();

            var cbFirst = HexHelp.DecodeHex(coinbaseFirst);
            var cbTail = HexHelp.DecodeHex(coinBaseSecond);
            var res = new Notification.NewJob(jobid, version, trie, cbFirst, cbTail, nbits, ntime, flush);
            HexHelp.DecodeInto(res.prevBlock.blob, prevHashHexstr);
            var decodedMerkles = merkles.Select(el => AsMerkle(el));
            res.merkles.AddRange(decodedMerkles);
            return res;
        }

        internal static double SetDifficulty(object? evargs)
        {
            if (null == evargs) throw new MissingRequiredException("mining.set_difficulty: no payload given");
            if (evargs is not JArray concrete) throw new MissingRequiredException("mining.set_difficulty: payload must be an array");
            if (concrete.Count != 1) throw new BadParseException("mining.set_difficulty: must have 1 element");
            var there = concrete[0];
            switch (there.Type)
            {
                case JTokenType.Integer:
                    {
                        var hopefully = (long)there; // hopefully won't miss data
                        return hopefully;
                    }
                case JTokenType.Float: return (double)there;
            }
            throw new BadParseException("mining.set_difficulty: unrecognized difficulty number type");
        }

        static Mining.Merkle AsMerkle(JToken maybe)
        {
            if (maybe.Type != JTokenType.String) throw new BadParseException("mining.notify: merkles must be strings");
            return AsMerkle(maybe.Value<string>());
        }

        static Mining.Merkle AsMerkle(string hex)
        {
            var res = new Mining.Merkle();
            HexHelp.DecodeInto(res.blob, hex);
            return res;
        }
    }
}
