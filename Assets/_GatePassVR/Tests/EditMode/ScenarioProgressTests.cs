// ScenarioProgress의 현재 Step 검증·1회 진행·완료·재시작 동작을 검증하는 EditMode 테스트
using GatePassVR.Core;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class ScenarioProgressTests
    {
        private static ScenarioProgress Create()
        {
            return new ScenarioProgress(new[] { "A", "B", "C" });
        }

        [Test]
        public void Starts_AtFirstStep_NotFinished()
        {
            var progress = Create();

            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.IsFalse(progress.IsFinished);
        }

        [Test]
        public void TryAdvance_WithCurrentId_MovesToNextStep()
        {
            var progress = Create();

            bool advanced = progress.TryAdvance("A");

            Assert.IsTrue(advanced);
            Assert.AreEqual(1, progress.CurrentIndex);
        }

        [Test]
        public void TryAdvance_WithOtherStepId_IsIgnored()
        {
            var progress = Create();

            bool advanced = progress.TryAdvance("B");

            Assert.IsFalse(advanced);
            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [Test]
        public void TryAdvance_SameIdTwice_AdvancesOnlyOnce()
        {
            var progress = Create();

            progress.TryAdvance("A");
            bool second = progress.TryAdvance("A");

            Assert.IsFalse(second);
            Assert.AreEqual(1, progress.CurrentIndex);
        }

        [Test]
        public void TryAdvance_LastStep_FinishesAndIgnoresFurtherReports()
        {
            var progress = Create();
            progress.TryAdvance("A");
            progress.TryAdvance("B");

            bool last = progress.TryAdvance("C");
            bool afterFinish = progress.TryAdvance("C");

            Assert.IsTrue(last);
            Assert.IsTrue(progress.IsFinished);
            Assert.IsFalse(afterFinish);
        }

        [Test]
        public void TryAdvance_NullOrEmptyId_IsIgnored()
        {
            var progress = new ScenarioProgress(new[] { "", "B" });

            Assert.IsFalse(progress.TryAdvance(null));
            Assert.IsFalse(progress.TryAdvance(""));
            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [Test]
        public void Restart_AfterFinish_ReturnsToFirstStep()
        {
            var progress = Create();
            progress.TryAdvance("A");
            progress.TryAdvance("B");
            progress.TryAdvance("C");

            progress.Restart();

            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.IsFalse(progress.IsFinished);
            Assert.IsTrue(progress.TryAdvance("A"));
        }
    }
}
