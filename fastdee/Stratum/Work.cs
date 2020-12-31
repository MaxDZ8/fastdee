using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Stratum
{
    class Work
    {
        internal readonly DifficultyTarget target;
        internal readonly IReadOnlyList<byte> header;
        internal readonly ShareSubmitInfo info;

        internal Work(DifficultyTarget target, IReadOnlyList<byte> header, ShareSubmitInfo info)
        {
            this.target = target;
            this.header = header;
            this.info = info;
        }
    }
}
