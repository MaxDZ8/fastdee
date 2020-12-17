using fastdee.Stratum;
using Xunit;
using Newtonsoft.Json;

namespace fastdee.Tests.Stratum
{
    public class NotificationSystemTests
    {
        [Fact]
        public void EmptyStringIsInvalidMethod()
        {
            var notifier = new NotificationSystem();
            Assert.Throws<MissingRequiredException>(() => notifier.Mangle("", null));
        }

        [Fact]
        public void MiningNotifyNewJobIsSupported()
        {
            // 'parameters' array of a truly observed mining.notify
            var trulyObserved = "[\"3ee4\",\"8e557df746001ce2f7be198521db1c2a3a5b5190c251ac9f0000093600000000\",\"01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff4e03af8e000401bccc5f08fabe6d6d00000000000000000000000000000000000000000000000000000000000000000100000000000000\",\"0f2f4d696e696e672d4475746368332f00000000020000000000000000266a24aa21a9ede2f61c3f71d1defd3fa999dfa36953755c690689799962b48bebd836974e8cf900c817a804000000160014923ae3df6b46c669e375f6389339adce9db0df6e00000000\",[],\"20000000\",\"1a0d53ab\",\"5fccbc01\",true]";
            var concrete = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(trulyObserved);
            var notifier = new NotificationSystem();
            Assert.True(notifier.Mangle("mining.notify", concrete));
        }
    }
}
