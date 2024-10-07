using System;
using System.Runtime.Serialization;

namespace fastdee.Stratum
{
    [Serializable]
    internal class LowScanRangeExhaustedException : Exception
    {
        public LowScanRangeExhaustedException()
        {
        }

        public LowScanRangeExhaustedException(string? message) : base(message)
        {
        }

        public LowScanRangeExhaustedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected LowScanRangeExhaustedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}