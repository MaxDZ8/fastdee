using System;
using System.Runtime.Serialization;

namespace fastdee
{
    [Serializable]
    internal class BadInitializationException : ApplicationException
    {
        public BadInitializationException()
        {
        }

        public BadInitializationException(string? message) : base(message)
        {
        }

        public BadInitializationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected BadInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}