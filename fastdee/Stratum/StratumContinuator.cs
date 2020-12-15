using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace fastdee.Stratum
{
    /// <summary>
    /// Provide a way to trigger responses: <see cref="MangleLine(string)"/>.
    /// </summary>
    class StratumContinuator : IRequestContinuator, IDisposable
    {
        readonly RequestCorrespondances matcher = new RequestCorrespondances();
        readonly ResponseParser parsers = new ResponseParser();
        readonly Func<string, Task> sendToServer;

        internal StratumContinuator(Func<string, Task> sendToServer) { this.sendToServer = sendToServer; }

        public async Task Subscribe(string version)
        {
            var args = new string[] { version };
            var send = matcher.Request("mining.subscribe", args, result => parsers.MiningSubscribe(result));
            var json = JsonConvert.SerializeObject(send);
            await sendToServer(json);
        }

        /// <summary>
        /// Attempts to guess if this is a request we understand and forward it to the right parser, awakening the sleeping process.
        /// </summary>
        internal bool MangleLine(string line)
        {
            var uglee = JsonConvert.DeserializeObject<JsonRpc.Response>(line);
            return matcher.Trigger(uglee);
        }

        public void Dispose()
        {
            matcher.Dispose();
        }
    }
}
