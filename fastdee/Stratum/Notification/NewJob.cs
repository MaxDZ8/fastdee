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
        public readonly List<byte> coinbaseInitial = new List<byte>();
        public readonly List<byte> coinbaseFinal = new List<byte>();
        public readonly List<Mining.MerkleRoot> merkles = new List<Mining.MerkleRoot>();
        public readonly uint blockVer;
        /// <summary>
        /// Aka 'nbits'.
        /// </summary>
        public readonly uint networkDiff;
        public readonly uint ntime;
        public readonly bool flush;

        public NewJob(string jobid, uint blockVer, uint networkDiff, uint ntime, bool flush)
        {
            this.jobid = jobid;
            this.blockVer = blockVer;
            this.networkDiff = networkDiff;
            this.ntime = ntime;
            this.flush = flush;
        }
    }
}
