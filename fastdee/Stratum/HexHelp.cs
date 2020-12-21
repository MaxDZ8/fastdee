namespace fastdee.Stratum
{
    public static class HexHelp
    {
        public static byte[] DecodeHex(string hex) {
            var blobby = new byte[hex.Length / 2];
            return DecodeInto(blobby, hex);
        }

        public static byte[] DecodeInto(byte[] fillme, string hex)
        {
            if (hex.Length == 0) throw new BadParseException("Hexadecimal strings cannot be empty");
            if (hex.Length % 2 != 0) throw new BadParseException("Hexadecimal strings must have even digit count");
            hex = hex.ToLowerInvariant();
            for (var loop = 0; loop < hex.Length; loop += 2) fillme[loop / 2] = HexValue(hex[loop + 0], hex[loop + 1]);
            return fillme;
        }

        public static byte HexValue(char hi, char lo) => (byte)((HexValue(hi) << 4) | HexValue(lo));

        public static byte HexValue(char digit)
        {
            if (digit >= '0' && digit <= '9') return (byte)(digit - '0');
            if (digit >= 'a' && digit <= 'f') return (byte)(10 + digit - 'a');
            throw new BadParseException("Invalid character in hex digit");
        }
    }
}
