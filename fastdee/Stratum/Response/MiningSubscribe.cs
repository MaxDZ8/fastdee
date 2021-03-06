﻿namespace fastdee.Stratum.Response
{
    /// <summary>
    /// This is not the request, it is the reply.
    /// 
    /// I have considered using c#9 records here. They are convenient as (for positionals) their constructors implicitly declare
    /// the interesting fields but they also generate things I don't really care about so this stays class.
    /// </summary>
    public class MiningSubscribe
    {
        public readonly string sessionId;
        public readonly byte[] extraNonceOne;
        public readonly ushort extraNonceTwoByteCount;

        public MiningSubscribe(string sessionId, byte[] extraNonceOne, ushort extraNonceTwoByteCount)
        {
            this.sessionId = sessionId;
            this.extraNonceOne = extraNonceOne;
            this.extraNonceTwoByteCount = extraNonceTwoByteCount;
        }
    }
}
