using Xunit;
using System;

namespace fastdee.Stratum.Tests
{
    public class WorkGeneratorTests
    {
        [Fact]
        public void InitiallyEmpty()
        {
            var uut = new WorkGenerator();
            Assert.True(uut.Empty);
        }

        [Fact]
        public void AttemptingConsumeWhenEmptyThrows()
        {
            var uut = new WorkGenerator();
            Assert.Throws<InvalidOperationException>(() => uut.WannaConsume(0));
        }

        [Fact]
        public void GivingHeaderIsNotEnough()
        {
            var uut = new WorkGenerator();
            uut.SetHeader(DummySubmit, new byte[] { 1 }); // that's certainly not a good header but it's not its concern
            Assert.Throws<InvalidOperationException>(() => uut.WannaConsume(0));
        }

        [Fact]
        public void GivingTargetIsNotEnough()
        {
            var uut = new WorkGenerator();
            var target = new DifficultyTarget();
            uut.SetTarget(target); // nonsensical difficulty... but that's still enough for it
            Assert.Throws<InvalidOperationException>(() => uut.WannaConsume(0));
        }

        [Fact]
        public void ReadyWhenHeaderAndDiffProvided()
        {
            var uut = new WorkGenerator();
            var target = new DifficultyTarget();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(target);
            Assert.False(uut.Empty);
        }

        [Fact]
        public void ConsumingZeroThrows()
        {
            var uut = new WorkGenerator();
            var target = new DifficultyTarget();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(target);
            Assert.Throws<ArgumentException>(() => uut.WannaConsume(0));
        }

        [Fact]
        public void ConsumingBurnsRequestedNonces()
        {
            var target = new DifficultyTarget();
            var uut = new WorkGenerator();
            var hdr = new byte[] { 1 };
            var info = DummySubmit;
            uut.SetHeader(info, hdr);
            uut.SetTarget(target);
            var work = uut.WannaConsume(1);
            Assert.Equal(1u, uut.ConsumedNonces);
            Assert.Same(work.target, target);
            Assert.Same(work.header, hdr); // and be careful, to save on copies it takes references to everything.
            Assert.Same(work.info, info);
        }

        [Fact]
        public void ConsumingToExhaustationThrows()
        {
            var uut = new WorkGenerator();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(new DifficultyTarget());
            Assert.Throws<ArgumentException>(() => uut.WannaConsume(2)); // you must roll a new nonce2 and feed me a new header.
            uut.WannaConsume(uint.MaxValue - 1);
        }

        [Fact]
        public void GivingSameHeaderDoesNotReset()
        {
            var uut = new WorkGenerator();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(new DifficultyTarget());
            uut.WannaConsume(uint.MaxValue - 2);
            uut.SetHeader(DummySubmit, new byte[] { 1 }); // it compares by value!
            Assert.Throws<ArgumentException>(() => uut.WannaConsume(3)); // you must roll a new nonce2 and feed me a new header.
        }

        [Fact]
        public void GivingNewHeaderResetsConsumedCount()
        {
            var uut = new WorkGenerator();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(new DifficultyTarget());
            uut.WannaConsume(uint.MaxValue - 50);
            uut.SetHeader(DummySubmit, new byte[] { 2 });
            Assert.Equal(0u, uut.ConsumedNonces);
        }

        [Fact]
        public void ReadyCanBeInterrupted()
        {
            var uut = new WorkGenerator();
            var target = new DifficultyTarget();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(target);
            uut.Stop();
            Assert.True(uut.Empty);
        }

        [Fact]
        public void InterruptedIsReadyWithJustHeader()
        {
            var uut = new WorkGenerator();
            var target = new DifficultyTarget();
            uut.SetHeader(DummySubmit, new byte[] { 1 });
            uut.SetTarget(target);
            uut.Stop();
            uut.SetHeader(DummySubmit, new byte[] { 2 });
            Assert.False(uut.Empty);
        }

        [Fact]
        public void CanSetStartingNonce()
        {
            var uut = new WorkGenerator();
            uut.NextNonce(0x12345678u);
            Assert.Equal(0x12345678u, uut.ConsumedNonces);
        }

        static ShareSubmitInfo DummySubmit => new ShareSubmitInfo("ueht", new byte[] { 1, 2, 3, 4 }, new byte[] { 0, 1, 23, 45 });
    }
}
