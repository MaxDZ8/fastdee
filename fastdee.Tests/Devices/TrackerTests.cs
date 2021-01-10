using fastdee.Devices;
using fastdee.Stratum;
using fastdee.PoolOps;
using Xunit;
using fastdee.Stratum.Notification;

namespace fastdee.Tests.Devices
{
    public class TrackerTests
    {
        /// <summary>
        /// The goal of the <see cref="Tracker"/> is to generate and track the work given to devices.
        /// Created trackers have no devices.
        /// </summary>
        [Fact]
        public void CanCreateEmpty()
        {
            var test = new Tracker<int>(MakeFedWorkGenerator().WannaConsume);
            Assert.Equal(0, test.DeviceCount);
        }

        /// <summary>
        /// Add devices by giving new addresses. The address is key.
        /// </summary>
        [Fact]
        public void AddedDevicesAreIdle()
        {
            var test = new Tracker<int>(MakeFedWorkGenerator().WannaConsume);
            test.ConsumeNonces(0, 123456);
            Assert.Equal(1, test.DeviceCount);
        }

        [Fact]
        public void DevicesAreByAddress()
        {
            var test = new Tracker<int>(MakeFedWorkGenerator().WannaConsume);
            test.ConsumeNonces(0, 123456);
            test.ConsumeNonces(1, 789000);
            test.ConsumeNonces(2, 101112);
            test.ConsumeNonces(0, 654321);
            Assert.Equal(3, test.DeviceCount);
        }

        [Fact]
        public void CanFlush()
        {
            var test = new Tracker<int>(MakeFedWorkGenerator().WannaConsume);
            test.ConsumeNonces(0, 123456);
            test.ConsumeNonces(1, 789000);
            test.ConsumeNonces(2, 101112);
            test.ConsumeNonces(0, 654321);
            test.FlushOldies(System.TimeSpan.Zero);
            Assert.Equal(0, test.DeviceCount);
        }

        [Fact]
        public void UnknownWorkIsNullWork()
        {
            var test = new Tracker<int>(MakeFedWorkGenerator().WannaConsume);
            var work = test.RetrieveOriginal(123);
            Assert.Null(work);
        }

        [Fact]
        public void GoodDeviceWorkHadOriginalWork()
        {
            var test = new Tracker<int>(MakeFedWorkGenerator().WannaConsume);
            var work = test.ConsumeNonces(0, 1234);
            var ori = test.RetrieveOriginal(work.wid);
            Assert.NotNull(ori);
        }

        static WorkGenerator MakeFedWorkGenerator()
        {
            var gen = new WorkGenerator();
            var target = new DifficultyTarget();
            gen.SetHeader(DummySubmit, new byte[] { 1 });
            gen.SetTarget(target);
            return gen;
        }

        static ShareSubmitInfo DummySubmit => new ShareSubmitInfo("ueht", new byte[] { 1, 2, 3, 4 }, new byte[] { 0, 1, 23, 45 });
    }
}
