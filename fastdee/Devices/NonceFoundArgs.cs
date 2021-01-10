namespace fastdee.Devices
{
    /// <summary>
    /// The device has found something, in case we are still interested!
    /// The device does not have full information. What it knows is simply how many nonces tested before match.
    /// </summary>
    /// <remarks>
    /// There's no need to understand which device sends this, <see cref="workid"/> allows the orchestrator to
    /// reconstruct all the data.
    /// </remarks>
    class NonceFoundArgs
    {
        /// <summary>
        /// Work identifier the server gave us to identify the algorithm-header sent to me.
        /// </summary>
        internal readonly ulong workid;
        /// <summary>
        /// The server knows the nonce base and has tracked it. How many nonces to add to find the match.
        /// </summary>
        internal readonly ulong increment;
        /// <summary>
        /// Some (most? all?) devices don't just give you the nonce, they also send you back the whole hash
        /// for correctness test!
        /// </summary>
        internal readonly byte[]? hash;

        public NonceFoundArgs(ulong workid, ulong increment, byte[]? hash)
        {
            this.workid = workid;
            this.increment = increment;
            this.hash = hash;
        }
    }
}
