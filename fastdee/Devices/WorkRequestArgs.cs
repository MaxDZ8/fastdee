namespace fastdee.Devices
{
    /// <summary>
    /// The device is asking for work.
    /// It said: hello, please allocate me ULONG hashes to scan and send me back an header in ALGO format.
    /// </summary>
    /// <typeparam name="A">Address type of the originating device.</typeparam>
    class WorkRequestArgs<A>
    {
        public readonly A originator;
        public readonly WireAlgoFormat algoFormat;
        /// <summary>
        /// In line of theory, a scan count of 4G is real small but in practice many algorithms are baked with 32bit
        /// scan ranges in mind. All things considered, I figured it is better to just stick with the basics for the time being,
        /// until a better usage model emerges.
        /// </summary>
        public readonly uint scanCount;

        public WorkRequestArgs(A originator, WireAlgoFormat algoFormat, uint reserve)
        {
            this.originator = originator;
            this.algoFormat = algoFormat;
            this.scanCount = reserve;
        }
    }
}
