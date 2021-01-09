namespace fastdee.Devices
{
    /// <summary>
    /// Various enumerations for the possible work a device could ask.
    /// The same algorithm might even have multiple implementations so this should be serialized at least
    /// as an ushort on the wire.
    /// </summary>
    enum WireAlgoFormat
    {
        Keccak = 0
    }
}
