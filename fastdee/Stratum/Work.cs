using System.Collections.Generic;

namespace fastdee.Stratum
{
    class Work
    {
        internal readonly DifficultyTarget target;
        internal readonly IReadOnlyList<byte> header;
        internal readonly ShareSubmitInfo info;
        internal readonly ulong uniq;
        internal readonly uint nonceBase;

        static internal ulong GeneratedSoFar => next;

        internal Work(DifficultyTarget target, IReadOnlyList<byte> header, ShareSubmitInfo info, uint nonceBase)
        {
            this.target = target;
            this.header = header;
            this.info = info;
            this.nonceBase = nonceBase;
            uniq = System.Threading.Interlocked.Increment(ref next);
        }

        static ulong next;
    }
}
