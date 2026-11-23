// HighlightState의 목표 유무, 5초 무반응 강화, 새 목표 시 초기화, 강조 세기 범위를 검증하는 EditMode 테스트
using GatePassVR.Guidance;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class HighlightStateTests
    {
        [Test]
        public void WithoutTarget_AmountIsZero_AndEscalateIgnored()
        {
            var state = new HighlightState();

            state.Escalate();

            Assert.AreEqual(HighlightLevel.Off, state.Level);
            Assert.AreEqual(0f, state.Evaluate(0.3f));
        }

        [Test]
        public void NewTarget_StartsAtNormal()
        {
            var state = new HighlightState();

            state.SetHasTarget(true);

            Assert.AreEqual(HighlightLevel.Normal, state.Level);
        }

        [Test]
        public void Escalate_WithTarget_GoesStrong()
        {
            var state = new HighlightState();
            state.SetHasTarget(true);

            state.Escalate();

            Assert.AreEqual(HighlightLevel.Strong, state.Level);
        }

        [Test]
        public void NewTarget_AfterEscalate_ResetsToNormal()
        {
            var state = new HighlightState();
            state.SetHasTarget(true);
            state.Escalate();

            state.SetHasTarget(true);

            Assert.AreEqual(HighlightLevel.Normal, state.Level);
        }

        [Test]
        public void RemovingTarget_TurnsOff()
        {
            var state = new HighlightState();
            state.SetHasTarget(true);
            state.Escalate();

            state.SetHasTarget(false);

            Assert.AreEqual(HighlightLevel.Off, state.Level);
        }

        [Test]
        public void Evaluate_StaysWithinZeroAndLevelMax()
        {
            var state = new HighlightState();
            state.SetHasTarget(true);

            float normalMax = 0f, strongMax = 0f;
            for (float t = 0f; t < 2f; t += 0.01f)
            {
                float v = state.Evaluate(t);
                Assert.That(v, Is.InRange(0f, 1f));
                normalMax = System.Math.Max(normalMax, v);
            }

            state.Escalate();
            for (float t = 0f; t < 2f; t += 0.01f)
            {
                float v = state.Evaluate(t);
                Assert.That(v, Is.InRange(0f, 1f));
                strongMax = System.Math.Max(strongMax, v);
            }

            Assert.Greater(normalMax, 0.1f);
            Assert.Greater(strongMax, normalMax);
        }
    }
}
