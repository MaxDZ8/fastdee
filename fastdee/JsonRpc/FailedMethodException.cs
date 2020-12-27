using System;

namespace fastdee.JsonRpc
{
    /// <summary>
    /// Thrown when a reply to a method has a non-null <see cref="Response.rawErr"/>.
    /// </summary>
    public class FailedMethodException : Exception
    {
        public object? payload;
    }
}
