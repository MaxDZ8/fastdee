using fastdee.Stratum;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// Orchestrating devices connected by UDP. The network is almost totally managed internally and the only data you need to provide
    /// is really a way to generate new work units.
    /// 1- Create
    /// 2- Bind (be sure to catch <see cref="SocketException"/>)
    /// 3- await <see cref="RunAsync"/>
    /// </summary>
    class Orchestrator : IDisposable
    {
        readonly Socket udpSock;
        readonly DatagramPump embeddedServer;

        public event EventHandler<WelcomedArgs<IPEndPoint, IPAddress>>? Welcomed;
        public event EventHandler<WorkProvidedArgs<IPEndPoint>>? WorkProvided;
        public event EventHandler<NonceFoundArgs> NonceFound
        {
            add { embeddedServer.NonceFound += value; }
            remove { embeddedServer.NonceFound -= value; }
        }

        public Orchestrator(System.Threading.CancellationToken goodbye)
        {
            udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            embeddedServer = new DatagramPump(udpSock, goodbye);
        }

        public void Bind(ushort port = 18458) => Bind(new IPEndPoint(IPAddress.Any, port));
        public void Bind(IPEndPoint landingIp) => udpSock.Bind(landingIp);

        /// <summary>
        /// This reliable function generates useful work to be given the asking device.
        /// It can return null to indicate no work to be given (useful for cooling down) but if it can, 
        /// it must take care of rolling everything required.
        /// </summary>
        /// <param name="originator">The requesting device</param>
        /// <param name="scanAmount">How many nonces to be reserved for the remote scan operation.</param>
        internal delegate RequestedWork? GenWorkFunc(IPEndPoint originator, ulong scanAmount);

        /// <summary>
        /// Before pumping the messages there is some additional setup to carry out.
        /// Mostly setting up the callbacks / events.
        /// </summary>
        internal Task RunAsync(GenWorkFunc genWork)
        {
            embeddedServer.IntroducedItself += (src, ev) =>
            {
                var myAddr = ReplyMaker.ResolveMyIpForDevice(ev.originator);
                if (null != myAddr)
                {
                    var blob = ReplyMaker.Welcome(myAddr, ev.identificator, ev.deviceSpecific);
                    if (null == blob) return;
                    lock (udpSock) udpSock.SendTo(blob, ev.originator);
                    Welcomed?.Invoke(this, new WelcomedArgs<IPEndPoint, IPAddress>(ev, myAddr));
                }
            };
            embeddedServer.WorkAsked += (src, ev) =>
            {
                if (ev.algoFormat != WireAlgoFormat.Keccak) return; // the idea is each fastdee instance runs a single algo
                var workUnit = genWork(ev.originator, ev.scanCount);
                if (null == workUnit) return;
                /* ^ The device will keep asking and it'll be quite noisy.
                 * All things considered I have decided this is the right approach because such traffic which gets increasingly common
                 * can be an indicator of something going awry. So, for the time being there's no "no work" reply. 
                 * Note idle devices over time will be more and more noisy and eventually reboot, going into discovery,
                 * that's even worse but when stuff breaks I want it to break big way.
                 */
                var payload = PayloadCooker.CookedPayload(workUnit, WireAlgoFormat.Keccak);
                var blob = ReplyMaker.YourWork(workUnit.wid, payload);
                lock (udpSock) udpSock.SendTo(blob, ev.originator);
                var given = new WorkProvidedArgs<IPEndPoint>(ev, workUnit);
                WorkProvided?.Invoke(this, given);
            };
            return embeddedServer.ReceiveForeverAsync();
        }

        public void Dispose()
        {
            udpSock.Dispose();
        }
    }
}
