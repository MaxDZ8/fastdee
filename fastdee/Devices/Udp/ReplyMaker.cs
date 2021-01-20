using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// The <see cref="EventInstantiator"/> mangles the data you received from the socket and, elevates it to real structures.
    /// The <see cref="ReplyMaker"/> is in some way the opposite, but slightly scaled up.
    /// It takes care of both building the reply and serializing it to a blob you send over the wire.
    /// </summary>
    class ReplyMaker
    {
        readonly IPAddress ipaddr;

        public ReplyMaker(IPAddress ipaddr)
        {
            this.ipaddr = ipaddr;
        }

        internal byte[]? Reply(byte[] identificator, byte[] deviceSpecific)
        {
            throw new NotImplementedException();
        }
    }
}
