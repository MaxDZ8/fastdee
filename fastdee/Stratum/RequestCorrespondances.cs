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

        internal (JsonRpc.Request, Task<T>) Request<T>(string methodName, object[] args, Func<object?, T> parser)
        {
            var id = System.Threading.Interlocked.Increment(ref msgid);
            var send = new JsonRpc.Request(id, methodName, args);
            var track = new Tracked<T>(parser);
            lock (pending)
            {
                pending.Add(id, track);
            }
            return (send, track.Task);
        }

        internal bool Trigger(JsonRpc.Response gotcha)
        {
            var subject = GetAndForget(gotcha.id);
            if (null == subject) return false;
            if (null != gotcha.rawRes) subject.Success(gotcha.rawRes);
            else if (null != gotcha.rawErr)
            {
                subject.Failure(new JsonRpc.FailedMethodException()
                {
                    payload = gotcha.rawErr
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
