using System.Collections.Generic;

namespace fastdee.Stratum
{
    /// <summary>
    /// Job data required to send back a share when found.
    /// </summary>
    class ShareSubmitInfo
    {
        public string JobId { get; }
        public IReadOnlyList<byte> Nonce2 { get; }
        public IReadOnlyList<byte> NetworkTime { get; }

        internal ShareSubmitInfo(string jobid, IReadOnlyList<byte> nonce2, IReadOnlyList<byte> ntime)
        {
            JobId = jobid;
            Nonce2 = nonce2;
            NetworkTime = ntime;
        }
    }
}
