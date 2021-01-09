namespace fastdee.Devices
{
    /// <summary>
    /// This is basically the reply to <see cref="WorkRequestEventArgs{A}"/>.
    /// Quirk: <see cref="WorkRequestEventArgs{A}"/> has a more complex role, it also tells who asked for work,
    /// by contrast, this is a pure payload to be given back.
    /// </summary>
    /// <remarks>
    /// I really wanted to make this a record but... it seems it doesn't interact nicely with
    /// documentation so I keep it old-style.
    /// </remarks>
    class RequestedWork
    {
        /// <summary>
        /// If the device finds a nonce, <see cref="NonceFoundEventArgs.workid"/> will have this value.
        /// By using this, the server knows the header, ntime, nonce2 and all the things.
        /// </summary>
        internal readonly uint wid;
        /// <summary>
        /// Header to be sent, in the format requested by <see cref="WorkRequestEventArgs{A}.algoFormat"/>.
        /// </summary>
        internal readonly byte[] header;

        internal RequestedWork(uint wid, byte[] header)
        {
            this.wid = wid;
            this.header = header;
        }
    }
}
