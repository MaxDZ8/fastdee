using System.Collections.Generic;

namespace fastdee.Stratum.Notification
{
    /// <summary>
    /// Stuff taken from "mining.notify"
    /// </summary>
    public class NewJob
    {
        public readonly string jobid;
        public readonly Mining.BlockHash prevBlock = new Mining.BlockHash();
        /// <summary>
        /// IDK what this is, but sgminer has it and it goes into block as well when present...
        /// </summary>
        public readonly byte[]? trie;
        public readonly byte[] cbHead;
        public readonly byte[] cbTail;
        public readonly List<Mining.Merkle> merkles = new List<Mining.Merkle>();

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

        public NewJob(string jobid, byte[] blockVer, byte[]? trie,
                      byte[] cbHead, byte[] cbTail,
                      byte[] networkDiff, byte[] ntime, bool flush)
        {
            this.jobid = jobid;
            this.blockVer = blockVer;
            this.trie = trie;
            this.cbHead = cbHead;
            this.cbTail = cbTail;
            this.networkDiff = networkDiff;
            this.ntime = ntime;
            this.flush = flush;
        }
    }
}
