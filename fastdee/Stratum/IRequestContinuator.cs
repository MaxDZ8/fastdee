using System.Threading.Tasks;

namespace fastdee.Stratum
{
    /// <summary>
    /// JSON-RPC calls to a targeted server. Keeps track of request ids and associates them to replies.
    /// </summary>
    interface IRequestContinuator
    {
        Task<Response.MiningSubscribe> Subscribe(string version);
    }
}
