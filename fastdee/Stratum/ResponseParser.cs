using System;

namespace fastdee.Stratum
{
    /// <summary>
    /// Isolates the various policies to turn magic result values into concrete instances.
    /// </summary>
    class ResponseParser
    {
        internal Response.MiningSubscribe MiningSubscribe(object? jsonLine)
        {
            throw new NotImplementedException(); // we apparently enjoy TDD
        }
    }
}
