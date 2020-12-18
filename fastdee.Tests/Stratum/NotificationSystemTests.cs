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
        public void MiningNotifyNewJobIsSupportedAndRaisesEvent_EmptyMerkles()
        {
            // 'parameters' array of a truly observed mining.notify - empty merkles array
            var trulyObserved = "[\"3ee4\",\"8e557df746001ce2f7be198521db1c2a3a5b5190c251ac9f0000093600000000\",\"01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff4e03af8e000401bccc5f08fabe6d6d00000000000000000000000000000000000000000000000000000000000000000100000000000000\",\"0f2f4d696e696e672d4475746368332f00000000020000000000000000266a24aa21a9ede2f61c3f71d1defd3fa999dfa36953755c690689799962b48bebd836974e8cf900c817a804000000160014923ae3df6b46c669e375f6389339adce9db0df6e00000000\",[],\"20000000\",\"1a0d53ab\",\"5fccbc01\",true]";
            var concrete = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(trulyObserved);
            var notifier = new NotificationSystem();
            fastdee.Stratum.Notification.NewJob job = null;
            notifier.NewJobReceived += (src, ev) => job = ev.newJob;
            Assert.True(notifier.Mangle("mining.notify", concrete));
            Assert.NotNull(job);
            Assert.Equal("3ee4", job.jobid);
            Assert.Equal(new byte[] {
                142, 85, 125, 247, 70, 0, 28, 226, 247, 190, 25, 133, 33, 219, 28, 42, 58, 91, 81, 144, 194, 81, 172, 159, 0, 0, 9, 54, 0, 0, 0, 0
            }, job.prevBlock.blob);
            Assert.Equal(new byte[] {
                1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255,
                255, 78, 3, 175, 142, 0, 4, 1, 188, 204, 95, 8, 250, 190, 109, 109, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0
            }, job.coinbaseInitial);
            Assert.Equal(new byte[] {
                15, 47, 77, 105, 110, 105, 110, 103, 45, 68, 117, 116, 99, 104, 51, 47, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 38, 106,
                36, 170, 33, 169, 237, 226, 246, 28, 63, 113, 209, 222, 253, 63, 169, 153, 223, 163, 105, 83, 117, 92, 105, 6, 137, 121, 153, 98,
                180, 139, 235, 216, 54, 151, 78, 140, 249, 0, 200, 23, 168, 4, 0, 0, 0, 22, 0, 20, 146, 58, 227, 223, 107, 70, 198, 105, 227, 117,
                246, 56, 147, 57, 173, 206, 157, 176, 223, 110, 0, 0, 0, 0
            }, job.coinbaseFinal);
            Assert.Empty(job.merkles);
            Assert.Equal((uint)2, job.blockVer);
            Assert.Equal((uint)93624, job.networkDiff);
            Assert.Equal((uint)138189, job.ntime);
            Assert.True(job.flush);
        }
    }
}
