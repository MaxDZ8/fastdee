using System.Threading.Tasks;

namespace fastdee.Devices
{
    /// <summary>
    /// The <see cref="ICommunicationsSource{T}"/> allows devices to talk to us.
    /// This one allows the orchestrator to talk to them.
    /// </summary>
    /// <typeparam name="T">Address type of the devices on this network.</typeparam>
    interface IReplySend<T>
    {
        Task ProvideWorkAsync(RequestedWork give);
    }
}
