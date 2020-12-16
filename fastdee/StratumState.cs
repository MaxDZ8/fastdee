using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee
{
    /// <summary>
    /// State reported by <see cref="Stratificator.Status"/>.
    /// </summary>
    enum StratumState
    {
        /// <summary>
        /// Just created. Program start. Nothing done so far.
        /// </summary>
        Pristine,
        /// <summary>
        /// Successfully resolved server endpoint and connecting to it.
        /// </summary>
        Connecting,
        /// <summary>
        /// Connection completed, sending "mining.subscribe" and waiting for corresponding reply.
        /// </summary>
        Subscribing,
        /// <summary>
        /// Got a first server reply, sending "mining.authorize". Some servers don't send back a reply
        /// but rather go straight into notificating new work.
        /// </summary>
        Authorizing,
        /// <summary>
        /// Got a new job notification from server and ready to provide dispatch data to kernels.
        /// </summary>
        GotWork,

        /// <summary>
        /// Something gone awry. Usually transport interrupted. Something will happen at some point.
        /// </summary>
        Failed
    }
}
