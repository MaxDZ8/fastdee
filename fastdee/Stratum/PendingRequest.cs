using System.Threading.Tasks;

namespace fastdee.Stratum
{
    /// <summary>
    /// Again, this in theory shouldn't be needed. I'm good with tuples in those specific cases.
    /// But it seems non-nullability information is lost, at least for the analyzer, bruh!
    /// </summary>
    internal class PendingRequest<T>
    {
        internal readonly JsonRpc.Request request;
        internal readonly Task<T> task;

        internal PendingRequest(JsonRpc.Request request, Task<T> task)
        {
            this.request = request;
            this.task = task;
        }
    }
}
