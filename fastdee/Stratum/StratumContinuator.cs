﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq; // ToArray

namespace fastdee.Stratum
{
    /// <summary>
    /// Provide a way to trigger responses: <see cref="MangleReply(ulong, object?, object?)"/>.
    /// </summary>
    public class StratumContinuator : IRequestContinuator, IDisposable
    {
        readonly RequestCorrespondances matcher = new RequestCorrespondances();
        readonly Func<string, Task> sendToServer;

        public StratumContinuator(Func<string, Task> sendToServer) { this.sendToServer = sendToServer; }

        public Task<Response.MiningSubscribe> SubscribeAsync(string version)
        {
            var args = new string[] { version };
            var req = matcher.Request("mining.subscribe", args, result => ResponseParser.MiningSubscribe(result));
            WrappedSend(req.request);
            return req.task;
        }

        public Task<bool> AuthorizeAsync(string user, string worker, string sillyPass)
        {
            var login = $"{user}.{worker}";
            var args = new string[] { login, sillyPass };
            var req = matcher.Request("mining.authorize", args, result => ResponseParser.MiningAuthorize(result));
            WrappedSend(req.request);
            return req.task;
        }

        internal Task<bool> SubmitAsync(string userName, string jobId, IReadOnlyList<byte> networkTime, IReadOnlyList<byte> nonce2, uint nonce)
        {
            var args = new string[] {
                userName, jobId,
                HexString(nonce2), HexString(networkTime), HexString(nonce)
            };
            var req = matcher.Request("mining.submit", args, result => ResponseParser.Submit(result));
            WrappedSend(req.request);
            return req.task;
        }

        void WrappedSend(object gizmo)
        {
            var json = JsonConvert.SerializeObject(gizmo);
            Task.Run(async () =>
            {
                try
                {
                    await sendToServer(json);
                }
                catch
                {
                    // Yeah, suppress everything (in the future: log)
                    // The task runtime typically doesn't like stuff being thrown without anyone catching!
                }
            });
        }

        static string HexString(IReadOnlyList<byte> blob)
        {
            var uglee = blob.ToArray();
            return BitConverter.ToString(uglee).Replace("-", "");
        }

        static string HexString(uint nonce)
        {
            var format = nonce > uint.MaxValue ? "x16" : "x8";
            return nonce.ToString(format);
        }

        /// <summary>
        /// Attempts to guess if this is a request we understand and forward it to the right parser, awakening the sleeping process.
        /// </summary>
        internal bool MangleReply(ulong id, object? result, object? error) => matcher.Trigger(id, result, error);

        public void Dispose()
        {
            matcher.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
