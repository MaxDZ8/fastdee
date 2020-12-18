﻿using System.Collections.Generic;

namespace fastdee.Stratum.Notification
{
    /// <summary>
    /// Stuff taken from "mining.notify"
    /// </summary>
    public class NewJob
    {
        public readonly string jobid;
        public readonly Mining.BlockHash prevBlock = new Mining.BlockHash();
        public readonly List<byte> coinbaseInitial = new List<byte>();
        public readonly List<byte> coinbaseFinal = new List<byte>();
        public readonly List<Mining.MerkleRoot> merkles = new List<Mining.MerkleRoot>();

        /// <summary>
        /// For some reason, M8M used this as an uint.
        /// It really isn't the case, everything about stratum is blind concatenation of strings.
        /// I don't concatenate the hex strings, I prefer to concatenate the binary values directly.
        /// </summary>
        public readonly byte[] blockVer;
        /// <summary>
        /// Aka 'nbits'.
        /// </summary>
        public readonly byte[] networkDiff;
        public readonly byte[] ntime;
        public readonly bool flush;

        public NewJob(string jobid, byte[] blockVer, byte[] networkDiff, byte[] ntime, bool flush)
        {
            this.jobid = jobid;
            this.blockVer = blockVer;
            this.networkDiff = networkDiff;
            this.ntime = ntime;
            this.flush = flush;
        }
    }
}
