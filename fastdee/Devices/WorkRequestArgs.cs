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
        public readonly ulong reserve;

        public WorkRequestArgs(A originator, WireAlgoFormat algoFormat, ulong reserve)
        {
            this.originator = originator;
            this.algoFormat = algoFormat;
            this.reserve = reserve;
        }
    }
}
