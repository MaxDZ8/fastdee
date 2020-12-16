using System;
using System.Threading.Tasks;

class GottaLineArgs : EventArgs
{
    public readonly string payload;

    internal GottaLineArgs(string payload) { this.payload = payload; }
}

namespace fastdee
{
    /// <summary>
    /// Json-rpc communication bidirectional abstraction.
    /// Send and receive strings.
    /// 
    /// Thread safety: implementations can either delegate protection of <see cref="WriteAsync(string)"/> calls to higher levels
    /// or protect internally. Strings must be sent "atomically".
    /// </summary>
    interface ILineChannel
    {
        /// <summary>
        /// Send a packet to the server. This channel is line-based so 1 packet = 1 line.
        /// Implementations terminate the line internally.
        /// Providing a string with internal newlines (ascii 10, 13, whatever unicode might consider newline)
        /// produces undefined results.
        /// </summary>
        Task WriteAsync(string raw);

        /// <summary>
        /// Collected enough data to form a line.
        /// A line includes at least one non-blank character.
        /// Implementation provide this free of the newline terminator.
        /// </summary>
        event EventHandler<GottaLineArgs> GottaLine;
    }
}
