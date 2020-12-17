using System.Threading.Tasks;

namespace fastdee.Stratum
{
    /// <summary>
    /// JSON-RPC calls to a targeted server. Keeps track of request ids and associates them to replies.
    /// </summary>
    interface IRequestContinuator
    {
        Task<Response.MiningSubscribe> SubscribeAsync(string version);

        /// <summary>
        /// Worker authorization takes some care. Some servers enjoy not giving a reply and providing work directly.
        /// In line of theory, I could just send and forget. Who would match the reply then?
        /// I can't even just cancel the thing, I would logically leak some memory.
        /// So, the call gives you the chance to explicitly complete the task by returning a function to call
        /// with the outcome you desire.
        /// </summary>
        /// <remarks>Does not return JUST a task, does it want -Async suffix?</remarks>
        PendingAuth Authorize(string user, string worker, string sillyPass);
    }
}
