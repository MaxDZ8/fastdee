using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fastdee.Stratum
{
    /// <summary>
    /// Build correspandances between json-rpc requests and their replies "generically".
    /// </summary>
    class RequestCorrespondances : IDisposable
    {
        interface ITracked
        {
            /// <summary>
            /// Called when the reply matched by id has a "result" field.
            /// </summary>
            void Success(object result);

            /// <summary>
            /// Called if the reply matched by id has a "error" field.
            /// </summary>
            void Failure(Exception ohno);
        }

        class Tracked<T> : ITracked
        {
            readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            readonly Func<object?, T> trasmogrify;

            internal Tracked(Func<object?, T> trasmogrify) { this.trasmogrify = trasmogrify; }

            public void Success(object result)
            {
                T magic;
                try
                {
                    magic = trasmogrify(result);
                }
                catch (Exception ex)
                {
                    Failure(ex);
                    return;
                }
                tcs.TrySetResult(magic);
            }

            /// <summary>
            /// Also called if the internal parse-and-cast operation goes awry.
            /// And it can itself boom.
            /// </summary>
            public void Failure(Exception ohno) => tcs.TrySetException(ohno);

            public Task<T> Task => tcs.Task;
        }

        ulong msgid;
        readonly Dictionary<ulong, ITracked> pending = new Dictionary<ulong, ITracked>();

        internal PendingRequest<T> Request<T>(string methodName, object[] args, Func<object?, T> parser)
        {
            var id = System.Threading.Interlocked.Increment(ref msgid);
            var send = new JsonRpc.Request(id, methodName, args);
            var track = new Tracked<T>(parser);
            lock (pending)
            {
                pending.Add(id, track);
            }
            return new PendingRequest<T>(send, track.Task);
        }

        /// <summary>
        /// Wake up a sleeping task (remote method call) by providing either a successful value or an error.
        /// </summary>
        /// <param name="id">Request identificator as got from the server.</param>
        /// <param name="result">Successful thing. Passed to your validator as is <see cref="Request{T}(string, object[], Func{object?, T})"/>.</param>
        /// <param name="error">Only relevant if no successful <paramref name="result"/> is given. If nonnull, will cause the request to fail.</param>
        /// <returns>False if <paramref name="id"/> matches no known request.</returns>
        /// <exception cref="NotImplementedException">You are trying to trigger with both <paramref name="result"/> and <paramref name="error"/> set to null.</exception>
        /// <remarks>
        /// If this either returns true or throws then the <paramref name="id"/> is consumed.
        /// Generally called from some other 'pumping' thread.
        /// </remarks>
        internal bool Trigger(ulong id, object? result, object? error)
        {
            var subject = GetAndForget(id);
            if (null == subject) return false;
            if (null != result) subject.Success(result);
            else if (null != error)
            {
                subject.Failure(new JsonRpc.FailedMethodException()
                {
                    payload = error
                });
            }
            else throw new NotImplementedException(); // ???
            return true;
        }

        ITracked? GetAndForget(ulong id)
        {
            lock (pending)
            {
                if (pending.TryGetValue(id, out var subject))
                {
                    // Apparently there's no atomic get-and-release in concurrent maps...
                    pending.Remove(id);
                    return subject;
                }
            }
            return null;
        }

        public void Dispose()
        {
            foreach (var el in pending)
            {
                // Not exactly, but kinda like it.
                el.Value.Failure(new TimeoutException());
            }
            pending.Clear(); // minor
        }
    }
}
