using fastdee.Stratum;
using System;
using System.Collections;
using System.Collections.Generic;

namespace fastdee.Devices
{
    /// <summary>
    /// The goal is to keep work data associated to each device.
    /// Each device works on a single scan, when it gives back a nonce I know how it has been generated.
    /// Up there a simple dictionary would be enough but to associate the work to the request, I need to generate the work!
    /// 
    /// And I also need to associate the new dispatched header to the original so I can recostruct the data,
    /// all this is better done in a single place.
    /// </summary>
    /// <typeparam name="T">Unique "address" of the worker in the network.</typeparam>
    class Tracker<T> where T : notnull
    {
        readonly Func<ulong, Work?> genWork;
        /// <summary>
        /// As always, this is going to be locked because I'm not sure I don't want group operations in the future.
        /// </summary>
        readonly Dictionary<T, DeviceData> known = new Dictionary<T, DeviceData>();

        /// <summary>
        /// I also keep in sync a list of work ids dispatched to which device.
        /// </summary>
        readonly Dictionary<uint, DeviceData> crunching = new Dictionary<uint, DeviceData>();

        /// <param name="genWork">
        /// Work generation callback. That's ideally <see cref="WorkGenerator.WannaConsume(ulong)"/> but that function is
        /// inappropriate as 1- does not roll nonce2 2- con throw if exhausted, and other bad things.
        /// The function can return null when no work is to be given - the device will go idle.
        /// </param>
        public Tracker(Func<ulong, Work?> genWork)
        {
            this.genWork = genWork;
        }

        /// <summary>
        /// How many devices we have seen so far. Maybe some went down but we don't know.
        /// </summary>
        /// <remarks>
        /// I will never understand why C# detests uints so much.
        /// </remarks>
        internal int DeviceCount
        {
            get
            {
                lock (known) return known.Count;
            }
        }
        /// <summary>
        /// As much as I am concerned, the device is 'working' if I have provided you a result to this call.
        /// If you fail to send over the wire, that's your concern.
        /// I also don't care about header format, as long as the dependancy from the work generator is there
        /// I'm happy with it.
        /// </summary>
        /// <returns>Null if no work available to be given.</returns>
        internal RequestedWork? ConsumeNonces(T originator, ulong scanCount)
        {
            var devInfo = GetCurrently(originator);
            RequestedWork cooked;
            uint? prev = null;
            lock (devInfo)
            {
                var work = genWork(scanCount);
                if (null == work) return null;
                var wid = (uint)work.uniq; // We hope to never have 2^32 work dispatch in flight!
                cooked = new RequestedWork(wid, work.header, work.target.TargD);
                var dispatching = new Dispatched(work, cooked);
                if (devInfo.dispatched != null) prev = devInfo.dispatched.provide.wid;
                devInfo.Working(dispatching);
            }
            lock (crunching)
            {
                if (prev.HasValue) crunching.Remove(prev.Value);
                crunching.Add(cooked.wid, devInfo);
            }
            return cooked;
        }

        /// <summary>
        /// Call me periodically to discard devices whose last communication is older than the argument
        /// (they went offline "permanently").
        /// </summary>
        /// <param name="ttl"></param>
        internal void FlushOldies(TimeSpan ttl)
        {
            // Do not modify the set while I'm iterating on it!
            var goners = new List<T>();
            var now = DateTime.UtcNow;
            lock (known)
            {
                foreach (var kv in known)
                {
                    lock (kv.Value)
                    {
                        var elapsed = now - kv.Value.lastReceived;
                        if (elapsed > ttl) goners.Add(kv.Key);
                    }
                }
                foreach (var farewell in goners) known.Remove(farewell);
            }
        }

        /// <summary>
        /// Some device sent back something! Awesome! How do we send it back to stratum?
        /// We need to get back the original data.
        /// </summary>
        /// <param name="wid">
        /// This sort-of-unique value corresponds to the originating <see cref="RequestedWork.wid"/>.
        /// </param>
        /// <returns>"Original" job information to send the share to stratum.</returns>
        internal Work? RetrieveOriginal(uint wid)
        {
            // Work id unknown. Kinda weird, but possible if you have multiple trackers and you just want to ask each of them blindly.
            if (false == crunching.TryGetValue(wid, out var device)) return null;
            // Maybe I should bail out in this case - inconsistent state / leaked? Should never happen.
            if (null == device.dispatched) return null;
            return device.dispatched.origin;
        }

        /// <summary>
        /// Retrieve current <see cref="DeviceData"/> associated to gived device.
        /// If not there, add it.
        /// <returns>The existing or just created instance.</returns>
        DeviceData GetCurrently(T addr)
        {
            lock (known)
            {
                if (known.TryGetValue(addr, out var there)) return there;
                var creat = new DeviceData(addr);
                known.Add(addr, creat);
                return creat;
            }
        }

        class DeviceData
        {
            internal readonly T addr;
            internal DateTime lastReceived;
            internal Dispatched? dispatched;

            internal DeviceData(T addr) { this.addr = addr; }

            internal void Working(Dispatched work)
            {
                lastReceived = DateTime.UtcNow;
                dispatched = work;
            }
        }

        /// <summary>
        /// Work dispatched to a device.
        /// </summary>
        class Dispatched
        {
            internal readonly DateTime generated = DateTime.UtcNow;
            internal readonly Work origin;
            internal readonly RequestedWork provide;

            internal Dispatched(Work origin, RequestedWork provide)
            {
                this.origin = origin;
                this.provide = provide;
            }
        }
    }
}
