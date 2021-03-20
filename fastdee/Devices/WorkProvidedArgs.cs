namespace fastdee.Devices
{
    /// <summary>
    /// Raised by the orchestrator after it sent new work to a device.
    /// </summary>
    /// <typeparam name="T">Device address type</typeparam>
    class WorkProvidedArgs<T>
    {
        internal WorkRequestArgs<T> OriginatingFrom { get; }
        internal RequestedWork WorkUnit { get; }

        internal WorkProvidedArgs(WorkRequestArgs<T> originator, RequestedWork wu)
        {
            OriginatingFrom = originator;
            WorkUnit = wu;
        }
    }
}
