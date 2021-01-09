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
    /// <typeparam name="T">Tipo indirizzo dei dispositivi gestiti.</typeparam>
    class Tracker<T>
    {
        readonly HeaderGenerator headerGenerator;

        public Tracker(HeaderGenerator headerGenerator)
        {
            this.headerGenerator = headerGenerator;
        }

        /// <summary>
        /// How many devices we have seen so far. Maybe some went down but we don't know.
        /// </summary>
        internal uint DeviceCount => throw new NotImplementedException();

        /// <summary>
        /// As much as I am concerned, the device is 'working' if I have provided you a result to this call.
        /// If you fail to send over the wire, that's your concern.
        /// I also don't care about header format, as long as the dependancy from the work generator is there
        /// I'm happy with it.
        /// </summary>
        internal RequestedWork ConsumeNonces(T originator, ulong scanCount)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call me periodically to discard devices whose last communication is older than the argument.
        /// </summary>
        /// <param name="ttl"></param>
        internal void FlushOldies(TimeSpan ttl)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
