// GuideReguideTimer의 5초/10초 재안내 판정과 Reset 동작을 검증하는 EditMode 테스트
using GatePassVR.Guidance;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class GuideReguideTimerTests
    {
        [Test]
        public void Tick_BeforeShortThreshold_DoesNotFire()
        {
            var timer = new GuideReguideTimer(shortThreshold: 5f, longThreshold: 10f);

            var (shortFired, longFired) = timer.Tick(4f);

            Assert.IsFalse(shortFired);
            Assert.IsFalse(longFired);
        }

        [Test]
        public void Tick_ReachingShortThreshold_FiresShortOnceOnly()
        {
            var timer = new GuideReguideTimer(shortThreshold: 5f, longThreshold: 10f);

            var (firstShort, firstLong) = timer.Tick(5f);
            var (secondShort, secondLong) = timer.Tick(1f);

            Assert.IsTrue(firstShort);
            Assert.IsFalse(firstLong);
            Assert.IsFalse(secondShort);
            Assert.IsFalse(secondLong);
        }

        [Test]
        public void Tick_ReachingLongThreshold_FiresLongOnceOnlyAndDoesNotRefireShort()
        {
            var timer = new GuideReguideTimer(shortThreshold: 5f, longThreshold: 10f);
            timer.Tick(5f); // short fires here

            var (firstShort, firstLong) = timer.Tick(5f); // elapsed = 10
            var (secondShort, secondLong) = timer.Tick(1f);

            Assert.IsFalse(firstShort);
            Assert.IsTrue(firstLong);
            Assert.IsFalse(secondShort);
            Assert.IsFalse(secondLong);
        }

        [Test]
        public void Tick_LargeDeltaCrossingBothThresholds_FiresBothInSameTick()
        {
            var timer = new GuideReguideTimer(shortThreshold: 5f, longThreshold: 10f);

            var (shortFired, longFired) = timer.Tick(10f);

            Assert.IsTrue(shortFired);
            Assert.IsTrue(longFired);
        }

        [Test]
        public void Reset_ClearsElapsedAndFiredFlags()
        {
            var timer = new GuideReguideTimer(shortThreshold: 5f, longThreshold: 10f);
            timer.Tick(10f);

            timer.Reset();

            Assert.AreEqual(0f, timer.ElapsedSinceProgress);
            Assert.IsFalse(timer.ShortFired);
            Assert.IsFalse(timer.LongFired);
        }
    }
}
