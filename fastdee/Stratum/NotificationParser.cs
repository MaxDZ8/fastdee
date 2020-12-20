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
            if (concrete.Count != 9) throw new BadParseException("mining.notify: element count mismatch, won't be able to parse");

            var jobid = concrete[0].Value<string>();
            var prevHashHexstr = concrete[1].Value<string>();
            var coinbaseFirst = concrete[2].Value<string>();
            var coinBaseSecond = concrete[3].Value<string>();
            var merkles = concrete[4] as JArray ?? throw new BadParseException("mining.notify: merkles must be an array");
            var version = HexHelp.DecodeHex(concrete[5].Value<string>());
            var nbits = HexHelp.DecodeHex(concrete[6].Value<string>());
            var ntime = HexHelp.DecodeHex(concrete[7].Value<string>());
            var flush = concrete[8].Value<bool>();

            var cbFirst = HexHelp.DecodeHex(coinbaseFirst);
            var cbTail = HexHelp.DecodeHex(coinBaseSecond);
            var res = new Notification.NewJob(jobid, version, cbFirst, cbTail, nbits, ntime, flush);
            HexHelp.DecodeInto(res.prevBlock.blob, prevHashHexstr);
            var decodedMerkles = merkles.Select(el => AsMerkle(el));
            res.merkles.AddRange(decodedMerkles);
            return res;
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
