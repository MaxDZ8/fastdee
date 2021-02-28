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
        readonly Tracker<IPEndPoint> tracker;

        public event EventHandler<WelcomedArgs<IPEndPoint, IPAddress>>? Welcomed;

        public Orchestrator(Func<ulong, Work?> genWork, System.Threading.CancellationToken goodbye)
        {
            udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            embeddedServer = new DatagramPump(udpSock, goodbye);
            tracker = new Tracker<IPEndPoint>(genWork);
        }

        public void Bind(ushort port = 18458) => Bind(new IPEndPoint(IPAddress.Any, port));
        public void Bind(IPEndPoint landingIp) => udpSock.Bind(landingIp);

        /// <summary>
        /// Before pumping the messages there is some additional setup to carry out.
        /// Mostly setting up the callbacks / events.
        /// </summary>
        internal Task RunAsync()
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
                var workUnit = tracker.ConsumeNonces(ev.originator, ev.scanCount);
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
                Console.WriteLine($"Gave {ev.originator.Address}:{ev.originator.Port} work unit {workUnit.wid}, scanning from {workUnit.nonceBase}, {ev.scanCount}");
            };
            embeddedServer.NonceFound += (src, ev) =>
            {
                var work = tracker.RetrieveOriginal(ev.workid);
                if (null == work)
                {
                    Console.WriteLine($"Worker providing results for untracked work ${ev.workid}, ignoring");
                    return;
                }
                ShowNonceInfo(work, ev.increment, ev.hash);
            };
            return embeddedServer.ReceiveForeverAsync();
        }

        static void ShowNonceInfo(Work work, ulong increment, byte[]? hash)
        {
            var nonce = work.nonceBase + increment;
            Console.WriteLine($"WU: {work.uniq}, found nonce: {nonce:x16}");
            if (null != hash && hash.Length != 0) { // if not null, len > 0 but let's check both.
                string hashString;
                if (hash.Length % 8 == 0) hashString = HashString(hash, 8);
                else if (hash.Length % 4 == 0) hashString = HashString(hash, 4);
                else hashString = BitConverter.ToString(hash).Replace("-", "");
                Console.WriteLine($"Hash: {hashString}");
            }
        }

        /// <summary>
        /// Show a few bytes without too much noise, yet help reading them.
        /// </summary>
        static string HashString(Span<byte> hash, uint width)
        {
            var bld = new System.Text.StringBuilder(hash.Length * 2 + 100); // plus some separators
            var count = 0;
            foreach (var oct in hash) {
                if (count != 0)
                {
                    if (count % 8 == 0 || count % width == 0) bld.Append(' ');
                    else if (count % 4 == 0) bld.Append('_');
                }
                bld.Append(oct.ToString("x2"));
                count++;
            }
            return bld.ToString();
        }

        public void Dispose()
        {
            udpSock.Dispose();
        }
    }
}
