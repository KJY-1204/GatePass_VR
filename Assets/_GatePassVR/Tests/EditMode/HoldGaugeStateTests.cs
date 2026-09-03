// HoldGaugeState의 진행/감소/완료/중복 방지/Reset 동작을 검증하는 EditMode 테스트
using GatePassVR.Interaction;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class HoldGaugeStateTests
    {
        [Test]
        public void Tick_WhileHovering_IncreasesProgress()
        {
            var gauge = new HoldGaugeState(holdDuration: 2f, decayRate: 1f);
            gauge.SetHovering(true);

            gauge.Tick(1f);

            Assert.AreEqual(0.5f, gauge.Progress, 0.0001f);
            Assert.IsFalse(gauge.IsCompleted);
        }

        [Test]
        public void Tick_WhileNotHovering_DecreasesProgress()
        {
            var gauge = new HoldGaugeState(holdDuration: 2f, decayRate: 1f);
            gauge.SetHovering(true);
            gauge.Tick(1f); // Progress = 0.5

            gauge.SetHovering(false);
            gauge.Tick(0.5f); // decayRate 1 -> 0.5 / 2 = 0.25 감소

            Assert.AreEqual(0.25f, gauge.Progress, 0.0001f);
        }

        [Test]
        public void Tick_ReachingFull_ReturnsTrueOnceAndMarksCompleted()
        {
            var gauge = new HoldGaugeState(holdDuration: 1f, decayRate: 1f);
            gauge.SetHovering(true);

            bool firstTick = gauge.Tick(0.5f);
            bool completedTick = gauge.Tick(0.6f);

            Assert.IsFalse(firstTick);
            Assert.IsTrue(completedTick);
            Assert.IsTrue(gauge.IsCompleted);
            Assert.AreEqual(1f, gauge.Progress, 0.0001f);
        }

        [Test]
        public void Tick_AfterCompleted_DoesNotFireAgainOrChangeProgress()
        {
            var gauge = new HoldGaugeState(holdDuration: 1f, decayRate: 1f);
            gauge.SetHovering(true);
            gauge.Tick(1f);

            gauge.SetHovering(false);
            bool firedAgain = gauge.Tick(5f);

            Assert.IsFalse(firedAgain);
            Assert.AreEqual(1f, gauge.Progress, 0.0001f);
        }

        [Test]
        public void Progress_NeverGoesBelowZero()
        {
            var gauge = new HoldGaugeState(holdDuration: 1f, decayRate: 1f);
            gauge.SetHovering(false);

            gauge.Tick(10f);

            Assert.AreEqual(0f, gauge.Progress, 0.0001f);
        }

        [Test]
        public void Reset_ClearsProgressHoveringAndCompleted()
        {
            var gauge = new HoldGaugeState(holdDuration: 1f, decayRate: 1f);
            gauge.SetHovering(true);
            gauge.Tick(1f);

            gauge.Reset();

            Assert.AreEqual(0f, gauge.Progress);
            Assert.IsFalse(gauge.IsHovering);
            Assert.IsFalse(gauge.IsCompleted);
        }
    }
}
