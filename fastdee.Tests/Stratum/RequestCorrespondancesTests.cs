using Xunit;
using fastdee.Stratum;
using System;
using System.Threading.Tasks;

namespace fastdee.Tests.Stratum
{
    public class RequestCorrespondancesTests
    {
        /// <summary>
        /// You can create request with any name, even if not quite ok by json-rpc standards, they are considered opaque.
        /// The first request has ID 1.
        /// </summary>
        [Fact]
        public void CanRequestWithoutArgs()
        {
            var stupidMethodName = "some.method-thisSTRINGisOpaque";
            using var uut = new RequestCorrespondances();
            var gen = uut.Request(stupidMethodName, Array.Empty<object>(), got => true);
            Assert.Same(stupidMethodName, gen.request.method);
            Assert.Equal(Array.Empty<object>(), gen.request.args);
            Assert.Equal(1u, gen.request.id);
        }

        /// <summary>
        /// The argument request array you provide is reused so don't mess with it.
        /// To a certain degree, consider it is being owned by the request object.
        /// </summary>
        [Fact]
        public void RequestOwnsArgs()
        {
            using var uut = new RequestCorrespondances();
            var sillyArgs = new object[] {
                true,
                1234
            };
            var gen = uut.Request("dontcare", sillyArgs, got => true);
            Assert.Same(sillyArgs, gen.request.args);
        }

        /// <summary>
        /// Triggering a non-existant request is nop and the <see cref="RequestCorrespondances.Trigger(ulong, object?, object?)"/>
        /// will return false.
        /// </summary>
        [Fact]
        public void TriggerUnknownIsNop()
        {
            using var uut = new RequestCorrespondances();
            var outcome = uut.Trigger(0, 123, null);
            Assert.False(outcome);
        }

        /// <summary>
        /// Triggering a non-existant request is nop even with errors.
        /// </summary>
        [Fact]
        public void TriggerUnknownIsNopEvenWithErrors()
        {
            using var uut = new RequestCorrespondances();
            var outcome = uut.Trigger(0, null, "irrelevant");
            Assert.False(outcome);
        }

        /// <summary>
        /// To make a call successful, provide a 'result' parameter to
        /// <see cref="RequestCorrespondances.Trigger(ulong, object?, object?)"/>. The value you provide must be compatible
        /// to your own expectations for the parser.
        /// </summary>
        [Fact]
        async public Task SuccessfulAsync()
        {
            var resPayload = "abcd";
            using var uut = new RequestCorrespondances();
            var req = uut.Request("nothing.really", Array.Empty<object>(), thing => thing as Gizmo ?? throw new InvalidCastException());
            var trig = uut.Trigger(req.request.id, new Gizmo() { payload = resPayload }, null);
            var result = await req.task;
            Assert.True(trig);
            Assert.Equal(resPayload, result.payload);
        }

        /// <summary>
        /// Your parser can still make the deferred method fail. Usually because the payload you get back isn't what you expect.
        /// In this case, the call is still considered successful but your await goes awry.
        /// </summary>
        [Fact]
        public async Task SuccessfulYetFailingAsync()
        {
            using var uut = new RequestCorrespondances();
            var req = uut.Request("nothing.really", Array.Empty<object>(), thing => thing as Gizmo ?? throw new InvalidCastException());
            var trig = uut.Trigger(req.request.id, 123, null);
            Assert.True(trig);
            await Assert.ThrowsAsync<InvalidCastException>(async () => await req.task);
        }



        /// <summary>
        /// Make a call fail by providing an 'error' parameter to
        /// <see cref="RequestCorrespondances.Trigger(ulong, object?, object?)"/>. Your await will throw 
        /// <see cref="JsonRpc.FailedMethodException"/>.
        /// The function completes succesfully anyway, returning true.
        /// </summary>
        [Fact]
        async public Task ErroredAsync()
        {
            using var uut = new RequestCorrespondances();
            var req = uut.Request("nothing.really", Array.Empty<object>(), thing => thing as Gizmo ?? throw new InvalidCastException());
            var trig = uut.Trigger(req.request.id, null, "gone wrong");
            Assert.True(trig);
            await Assert.ThrowsAsync<JsonRpc.FailedMethodException>(async () => await req.task);
        }

        /// <summary>
        /// Trying to trigger a call with neither a result nor an error will throw <see cref="NotImplementedException"/>.
        /// This means the <see cref="RequestCorrespondances.Trigger(ulong, object?, object?)"/> doesn't support null as value.
        /// </summary>
        [Fact]
        public void InvalidTrigger()
        {
            using var uut = new RequestCorrespondances();
            var req = uut.Request("nothing.really", Array.Empty<object>(), thing => thing as Gizmo ?? throw new InvalidCastException());
            Assert.Throws<NotImplementedException>(() => uut.Trigger(req.request.id, null, null));
        }


        class Gizmo
        {
            internal string payload = "bruh";
        }
    }
}
