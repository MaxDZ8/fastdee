namespace fastdee.Devices
{
    /// <summary>
    /// For protocols where device sends an initial hello to be instructed about orchestrator address,
    /// hello can also be used to deliver some more device-specific information.
    /// 
    /// Contents are two arbitrary blobs, both are basically opaque but they are slightly different in scope.
    /// </summary>
    /// <typeparam name="A">Type of the address in use by this network type.</typeparam>
    class TurnOnArgs<A>
    {
        public readonly A originator;

        /// <summary>
        /// In line of principle this identifies stuff such as the device model, producer, ect...
        /// It's kind of an header as we can use it to decide how to parse <see cref="deviceSpecific"/>.
        /// </summary>
        internal byte[] identificator;
        /// <summary>
        /// Arbitrary data. It would be best for the parsing to be a function of <see cref="identificator"/>,
        /// but more structured parsing is allowed.
        /// </summary>
        internal byte[] deviceSpecific;

        internal TurnOnArgs(A originator, byte[] identificator, byte[] deviceSpecific)
        {
            this.originator = originator;
            this.identificator = identificator;
            this.deviceSpecific = deviceSpecific;
        }
    }
}
