using System;

namespace fastdee.Devices
{
    /// <summary>
    /// Worked data somehow comes and after packing it is emitted as an event.
    /// I can think of at least 3 interesting sources:
    /// - First candidate is to drive them by Ethernet or some other kind of IP-based network.
    /// - I expect to drive workers by SPI in the future (note SPI only supports controller->device communication,
    ///   so this is particularly weird as it needs to be pumped).
    /// - If I fully enjoy playing with this I might also pop I2C or be more esoteric (868Mhz? LoRa? You name it!)
    /// 
    /// In all cases, this interface must support client identification so we can reply back to it.
    /// </summary>
    /// <typeparam name="T">Address type for the devices.</typeparam>
    interface ICommunicationsSource<T>
    {
        /// <summary>
        /// Reply with a <see cref="RequestedWork"/>.
        /// </summary>
        event EventHandler<WorkRequestArgs<T>>? WorkAsked;
        /// <summary>
        /// No reply required. The devices send those to the orchestrator a few times and they're done.
        /// </summary>
        event EventHandler<NonceFoundArgs>? NonceFound;
    }
}
