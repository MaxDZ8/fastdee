using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Stratum
{
    /// <summary>
    /// Yes, tuples are awesome.
    /// But... if I use them reference nullability is garbled.
    /// ???
    /// 
    /// So have this.
    /// </summary>
    public class PendingAuth
    {
        /// <summary>
        /// Completes when "mining.authorize" receives a reply. Be it success or error.
        /// </summary>
        public readonly Task<bool> task;

        /// <summary>
        /// Have you got work already? Feel free to implicitly complete <see cref="task"/> by handing me a value.
        /// </summary>
        public readonly Action<bool> trigger;

        internal PendingAuth(Task<bool> task, Action<bool> trigger)
        {
            this.task = task;
            this.trigger = trigger;
        }
    }
}
