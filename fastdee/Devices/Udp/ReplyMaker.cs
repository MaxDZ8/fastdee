using System;
using System.Collections.Generic;
using System.Net;

namespace fastdee.Devices.Udp
{
    /// <summary>
    /// The <see cref="EventInstantiator"/> mangles the data you received from the socket and, elevates it to real structures.
    /// The <see cref="ReplyMaker"/> is in some way the opposite, but slightly scaled up.
    /// It takes care of both building the reply and serializing it to a blob you send over the wire.
    /// </summary>
    static class ReplyMaker
    {
        /// <summary>
        /// Building a reply is all fine and dandy but while we know who talked to us, we don't know what address
        /// the talker can use to contact the orchestrator with an unicast address!
        /// 
        /// I could certainly rework the protocol to work in broadcast (at least in device to controller direction)
        /// but I find more valuable to resolve my local address by the point of view of the <paramref name="originator"/>.
        /// </summary>
        /// <returns>My network address, from the point of view of the originator. If I can't match a network I'll return null.</returns>
        /// <remarks>
        /// If I bind the UDP socket to <see cref="IPAddress.Any"/> it'll just report 0.0.0.0 and be done.
        /// OFC that's not anything we want to tell a device.
        /// </remarks>
        internal static IPAddress? ResolveMyIpForDevice(IPEndPoint originator)
        {
            // First I need to understand from which interface the packet comes from.
            // Simply evaluating the addresses isn't quite enough as I don't know the netmasks.
            var networks = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            Span<byte> nicBlob = stackalloc byte[4];
            var originAddr = UintFromIp4(originator.Address);
            foreach (var nic in networks)
            {
                var ipProps = nic.GetIPProperties();
                foreach (var unicast in ipProps.UnicastAddresses)
                {
                    var addr = unicast.Address;
                    if (null == addr) continue; // I assume it's just the CLR is not nullable-annotated yet
                    if (addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) continue; // for the time being, keep it on IPv4
                    var mask = UintFromIp4(unicast.IPv4Mask);
                    var oriMasked = originAddr & mask;
                    var selfMasked = UintFromIp4(addr) & mask;
                    if (oriMasked != selfMasked) continue; // different subnets, not interesting for sure
                    return addr;
                }
            }
            return null;
        }

        static uint UintFromIp4(IPAddress addr)
        {
            Span<byte> blob = stackalloc byte[4];
            if (!addr.TryWriteBytes(blob, out var written) || written != 4)
            {
                throw new ArgumentException("it doesn't look like a decent IPv4 addr", nameof(addr));
            }
            return UintFromIp4(blob);
        }

        static uint UintFromIp4(Span<byte> blob)
        {
            var magic = blob[0] | blob[1] << 8 | blob[2] << 16 | blob[3] << 24;
            return (uint)magic; // I don't care about endieness as long as they compare the same coherently
        }

        /// <summary>
        /// Construct the reply to <see cref="TurnOnArgs{A}"/>. In line of principle you could not-reply to
        /// some devices as they could be mangled by somebody else but... for the time being it's just a matter
        /// of giving back the server address to perform unicast packet transmissions.
        /// </summary>
        /// <param name="reachme">An unicast address the device who originally talked can contact me.</param>
        /// <param name="identificator">Currently unused.</param>
        /// <param name="deviceSpecific">Currently unused.</param>
        /// <returns>
        /// Blob to send over the wire. I won't touch it anymore. If I don't want to deal with this device
        /// (maybe it belongs to another server) I will return null and you send nothing to it.
        /// </returns>
        static internal byte[]? Welcome(IPAddress reachme, byte[] identificator, byte[] deviceSpecific)
        {
            var addrBlob = reachme.GetAddressBytes();
            var blob = new byte[1 + 1 + addrBlob.Length];
            var store = StoreKind(blob, OutgoingKind.ServerAddress);
            store = StoreByte(store, 0); // this is really a bitflag of features. For the time being it must be zero so no big deal
            store = StoreByteArray(store, addrBlob);
            ThrowIfNotFullyConsumed(store);
            return blob;
        }

        /// <summary>
        /// Serialize a work unit so it goes over the wire to a device.
        /// The structure must be common to all devices and algorithm implementations but the header can really be
        /// arbitrary payload as long as it's in the format the device expects.
        /// </summary>
        /// <param name="work">The work unit to provide. If you want to leave the device idle don't call me.</param>
        /// <returns>Opaque blob to send back to device.</returns>
        static internal byte[] YourWork(RequestedWork work)
        {
            var blob = new byte[1 + 4 + work.header.Count + 8]; // pkt_kind, wid, payload, diff_threshold_low
            var store = StoreKind(blob, OutgoingKind.WorkUnit);
            store = StoreUint(store, work.wid);
            store = StoreByteList(store, work.header);
            store = StoreUlong(store, work.diffThreshold);
            ThrowIfNotFullyConsumed(store);
            return blob;
        }


        /// <summary>
        /// Convenience.
        /// </summary>
        static Span<byte> StoreKind(Span<byte> buff, OutgoingKind kind) => StoreByte(buff, (byte)kind);

        static Span<byte> StoreByte(Span<byte> buff, byte value)
        {
            buff[0] = value;
            return buff[1..];
        }

        static Span<byte> StoreByteArray(Span<byte> buff, ReadOnlySpan<byte> value)
        {
            for (var cp = 0; cp < value.Length; cp++) buff[cp] = value[cp];
            return buff[value.Length..];
        }

        // TODO: maybe just IEnumerable?
        static Span<byte> StoreByteList(Span<byte> buff, IReadOnlyList<byte> value)
        {
            for (var cp = 0; cp < value.Count; cp++) buff[cp] = value[cp];
            return buff[value.Count..];
        }

        static Span<byte> StoreUint(Span<byte> buff, uint value)
        {
            buff[0] = (byte)(value);
            buff[1] = (byte)(value >> 8);
            buff[2] = (byte)(value >> 16);
            buff[3] = (byte)(value >> 24);
            return buff[4..];
        }

        static Span<byte> StoreUlong(Span<byte> buff, ulong value)
        {
            var hi = (uint)(value >> 32);
            var lo = (uint)(value);
            buff = StoreUint(buff, lo);
            buff = StoreUint(buff, hi);
            return buff;
        }

        static void ThrowIfNotFullyConsumed(Span<byte> buff)
        {
            if (buff.IsEmpty) return;
            throw new ArgumentException($"{buff.Length} bytes not consumed", nameof(buff));
        }
    }
}
