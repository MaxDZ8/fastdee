using System.Threading.Tasks;
using Xunit;

namespace fastdee.Stratum.Tests
{
    public class StratumContinuatorTests
    {
        [Fact]
        public async Task CanAuthorizeImplicitly()
        {
            // It completes even without no pumping at all.
            string sent = "";
            var uut = new StratumContinuator(str =>
            {
                sent = str;
                return Task.CompletedTask;
            });
            var pending = uut.Authorize("username", "workeruniq", "wannabePass");
            await Task.WhenAll(pending.task, Task.Run(async () =>
            {
                await Task.Delay(1);
                pending.trigger(true);
            }));
            Assert.True(pending.task.IsCompleted);
            Assert.True(pending.task.Result);
        }
    }
}
