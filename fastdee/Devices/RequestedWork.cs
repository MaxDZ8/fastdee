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

        internal RequestedWork(uint wid, IReadOnlyList<byte> header)
        {
            this.wid = wid;
            this.header = header;
        }
    }
}
