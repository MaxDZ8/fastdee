using fastdee.Stratum;
using Xunit;
using Newtonsoft.Json;
using System.Linq;

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
            }, job.cbHead);
            Assert.Equal(new byte[] {
                15, 47, 77, 105, 110, 105, 110, 103, 45, 68, 117, 116, 99, 104, 51, 47, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 38, 106,
                36, 170, 33, 169, 237, 226, 246, 28, 63, 113, 209, 222, 253, 63, 169, 153, 223, 163, 105, 83, 117, 92, 105, 6, 137, 121, 153, 98,
                180, 139, 235, 216, 54, 151, 78, 140, 249, 0, 200, 23, 168, 4, 0, 0, 0, 22, 0, 20, 146, 58, 227, 223, 107, 70, 198, 105, 227, 117,
                246, 56, 147, 57, 173, 206, 157, 176, 223, 110, 0, 0, 0, 0
            }, job.cbTail);
            Assert.Empty(job.merkles);
            Assert.Equal(new byte[] { 0x20, 0x00, 0x00, 0x00 }, job.blockVer);
            Assert.Equal(new byte[] { 0x1a, 0x0d, 0x53, 0xab }, job.networkDiff);
            Assert.Equal(new byte[] { 0x5f, 0xcc, 0xbc, 0x01 }, job.ntime);
            Assert.True(job.flush);
        }

        [Fact]
        public void MiningNotifyNewJobSupportedAndRaisesEvent_WithMerkles()
        {
            // Got from the very same server several minutes later.
            // Also, it is super hard to get more than a merkle on this pool... weird!
            var observed = "[\"3fe5\",\"22bf14955e058df426ba07578b4078fab6904f6ee2db624b0000012400000000\",\"01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff4e03c28e00048deccc5f08fabe6d6d00000000000000000000000000000000000000000000000000000000000000000100000000000000\",\"0f2f4d696e696e672d4475746368332f00000000020000000000000000266a24aa21a9ed0c88f3e56491004bc53035fa9eb202d8d4d2e36a5044daf5c3118eba48abee76adcc17a804000000160014923ae3df6b46c669e375f6389339adce9db0df6e00000000\",[\"4a29adf239ef07fa349b3fbe3052486e59bea9ecc669a1b50e3f17fa7d294446\"],\"20000000\",\"1a0d53ab\",\"5fccec8d\",false]";
            var concrete = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(observed);
            var notifier = new NotificationSystem();
            fastdee.Stratum.Notification.NewJob job = null;
            notifier.NewJobReceived += (src, ev) => job = ev.newJob;
            Assert.True(notifier.Mangle("mining.notify", concrete));
            Assert.NotNull(job);
            Assert.Equal("3fe5", job.jobid);
            Assert.Equal(new byte[] { 34, 191, 20, 149, 94, 5, 141, 244, 38, 186, 7, 87, 139, 64, 120, 250, 182, 144, 79, 110, 226, 219, 98, 75, 0, 0, 1, 36, 0, 0, 0, 0 }, job.prevBlock.blob);
            Assert.Equal(new byte[] {
                1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 78, 3, 194, 142, 0, 4, 141,
                236, 204, 95, 8, 250, 190, 109, 109, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0
            }, job.cbHead);
            Assert.Equal(new byte[] {
                15, 47, 77, 105, 110, 105, 110, 103, 45, 68, 117, 116, 99, 104, 51, 47, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 38, 106, 36, 170, 33, 169, 237, 12, 136, 243, 229, 100, 145, 0,
                75, 197, 48, 53, 250, 158, 178, 2, 216, 212, 210, 227, 106, 80, 68, 218, 245, 195, 17, 142, 186, 72, 171, 238, 118, 173, 204, 23, 168, 4, 0, 0, 0, 22, 0, 20, 146, 58, 227, 223,
                107, 70, 198, 105, 227, 117, 246, 56, 147, 57, 173, 206, 157, 176, 223, 110, 0, 0, 0, 0
            }, job.cbTail);
            Assert.Single(job.merkles);
            Assert.Equal(new byte[] {
                74, 41, 173, 242, 57, 239, 7, 250, 52, 155, 63, 190, 48, 82, 72, 110, 89, 190, 169, 236, 198, 105, 161, 181, 14, 63, 23, 250, 125, 41, 68, 70
            }, job.merkles[0].blob);
            Assert.Equal(new byte[] { 0x20, 0x00, 0x00, 0x00 }, job.blockVer);
            Assert.Equal(new byte[] { 0x1a, 0x0d, 0x53, 0xab }, job.networkDiff);
            Assert.Equal(new byte[] { 0x5f, 0xcc, 0xec, 0x8d }, job.ntime);
            Assert.False(job.flush);
        }

        [Fact]
        public void ServerCanSetIntegerDifficulty()
        {
            var observed = "[128]";
            var concrete = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(observed);
            var notifier = new NotificationSystem();
            var diff = 0.0;
            notifier.DifficultyReceived += (src, ev) => diff = ev.difficulty;
            var methodName = fastdee.Stratum.Notification.SetDifficulty.CommandString;
            var processed = notifier.Mangle(methodName, concrete);

            Assert.True(processed);
            Assert.Equal(128.0, diff);
        }

        [Fact]
        public void ServerCanSetFloatingDifficulty()
        {
            var observed = "[48.24]";
            var concrete = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JArray>(observed);
            var notifier = new NotificationSystem();
            var diff = 0.0;
            notifier.DifficultyReceived += (src, ev) => diff = ev.difficulty;
            var methodName = fastdee.Stratum.Notification.SetDifficulty.CommandString;
            var processed = notifier.Mangle(methodName, concrete);

            Assert.True(processed);
            Assert.Equal(48.24, diff);
        }
    }
}
