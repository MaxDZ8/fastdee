using System.Collections.Generic;

namespace fastdee.Devices
{
    /// <summary>
    /// This is basically the reply to <see cref="WorkRequestArgs{A}"/>.
    /// Quirk: <see cref="WorkRequestArgs{A}"/> has a more complex role, it also tells who asked for work,
    /// by contrast, this is a pure payload to be given back.
    /// </summary>
    /// <remarks>
    /// I really wanted to make this a record but... it seems it doesn't interact nicely with
    /// documentation so I keep it old-style.
    /// </remarks>
    class RequestedWork
    {
        /// <summary>
        /// If the device finds a nonce, <see cref="NonceFoundArgs.workid"/> will have this value.
        /// By using this, the server knows the header, ntime, nonce2 and all the things.
        /// </summary>
        internal readonly uint wid;
        /// <summary>
        /// Header to be sent, in the format requested by <see cref="WorkRequestArgs{A}.algoFormat"/>.
        /// </summary>
        internal readonly IReadOnlyList<byte> header;
        /// <summary>
        /// I initially considered having the hardware work at constant difficulty
        /// but it is just convenient to have it programmable.
        /// I'm not totally sure how it is supposed to go by but for the time being I'll stick with 64-bit.
        /// Those are usually from the lowest bits of the target.
        /// </summary>
        internal readonly ulong diffThreshold;

        internal RequestedWork(uint wid, IReadOnlyList<byte> header, ulong diffThreshold)
        {
            this.wid = wid;
            this.header = header;
            this.diffThreshold = diffThreshold;
        }
    }
}
