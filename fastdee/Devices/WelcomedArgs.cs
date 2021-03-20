namespace fastdee.Devices
{
    /// <summary>
    /// Raised after the orchestrator welcomed a device by sending back the IP.
    /// </summary>
    class WelcomedArgs<D, M>
    {
        /// <summary>
        /// This is the original event holding the low-level data received from the joining device.
        /// </summary>
        internal TurnOnArgs<D> OriginatingFrom { get; }

        /// <summary>
        /// This is the answer the orchestrator already sent back.
        /// </summary>
        internal M Address { get; }

        internal WelcomedArgs(TurnOnArgs<D> originator, M address)
        {
            OriginatingFrom = originator;
            Address = address;
        }
    }
}
