using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// There are various ways to manage packet types over the wire.
    /// One is to have a single enumeration for both "in" and "out". That makes parsing real easy for devices
    /// (just get a number and you're good to go). It's also fairly flexible as it blends well with peer-to-peer traffic.
    /// 
    /// When we have only 8 bits and a clear device-controller hierarchy however, having different meanings for the same
    /// value (depending on direction) is also acceptable.
    /// 
    /// I sometimes call those packets "outgoing" (to devices, those are there) and "inbound" (from devices) but this time
    /// I decided to do differently with <see cref="PacketKind"/>. Because you know, reasons.
    /// </summary>
    enum OutgoingKind
    {
        ServerAddress = 0
    }
}
