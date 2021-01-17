using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// Datagram type, usually serialized as 8 bits.
    /// </summary>
    enum PacketKind
    {
        /// <summary>
        /// Broadcasted by devices coming online, reply with server IP address.
        /// </summary>
        HelloOnline = 0,
        /// <summary>
        /// An idle device requesting for some work. Reserve the requested scan range and send an adjusted header.
        /// </summary>
        GimmeWork = 1,
        /// <summary>
        /// A working device has found this and is sending back to orchestrator for submitting.
        /// </summary>
        FoundNonce = 2
    }
}
