using System;

namespace fastdee
{
    /// <summary>
    /// Some algorithms are fairly quirky when it comes to mangling extranonce2.
    /// Incapsulate all the differences.
    /// </summary>
    /// <remarks>
    /// Note the only modifying calls are <see cref="Reset"/> and <see cref="Consumed"/>.
    /// Others are just pure reads.
    /// </remarks>
    public interface IExtraNonce2Provider
    {
        /// <summary>
        /// Returns the amount of bytes taken by the produced nonce.
        /// Must be the same value as asked by the pool. Usually 4. Consider it kinda constant.
        /// </summary>
        int ByteCount { get; }

        /// <summary>
        /// Tell me the current value, in machine-native endianess.
        /// </summary>
        ulong NativeValue { get; }

        /// <summary>
        /// Slap the current nonce2 in little endian encoding to the buffer provided.
        /// </summary>
        void CopyIntoBuffer(Span<byte> coinbaseSlice);

        /// <summary>
        /// To be called in response to <see cref="Stratum.Notification.NewJob.flush"/>.
        /// Resets the nonce to 0.
        /// </summary>
        void Reset();

        /// <summary>
        /// If you generated a new work unit, mostly after you called <see cref="CopyIntoBuffer(Span{byte})"/> you most likely
        /// want a new nonce2 to be used by calling this.
        /// </summary>
        void Consumed();
    }
}
