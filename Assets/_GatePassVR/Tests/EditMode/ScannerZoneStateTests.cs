// ScannerZoneState의 순서 준수·머무름 시간·1회 성공·틀린 물건·Reset 복구 동작을 검증하는 EditMode 테스트
using GatePassVR.Interaction;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class ScannerZoneStateTests
    {
        private static ScannerZoneState Create()
        {
            return new ScannerZoneState(itemCount: 2, scanTime: 0.5f);
        }

        [Test]
        public void ExpectedItem_StayingForScanTime_ScansOnceAndMovesToNext()
        {
            var state = Create();

            var early = state.Update(expectedInside: true, otherInside: false, deltaTime: 0.3f);
            var done = state.Update(expectedInside: true, otherInside: false, deltaTime: 0.3f);
            var after = state.Update(expectedInside: false, otherInside: false, deltaTime: 0.3f);

            Assert.AreEqual(ScanResult.None, early);
            Assert.AreEqual(ScanResult.Scanned, done);
            Assert.AreEqual(ScanResult.None, after);
            Assert.AreEqual(1, state.NextIndex);
        }

        [Test]
        public void ExpectedItem_LeavingBeforeScanTime_RestartsDwell()
        {
            var state = Create();

            state.Update(expectedInside: true, otherInside: false, deltaTime: 0.4f);
            state.Update(expectedInside: false, otherInside: false, deltaTime: 0.1f);
            var result = state.Update(expectedInside: true, otherInside: false, deltaTime: 0.4f);

            Assert.AreEqual(ScanResult.None, result);
            Assert.AreEqual(0, state.NextIndex);
        }

        [Test]
        public void OtherItem_ReportsWrongOncePerEntry_AndNeverScans()
        {
            var state = Create();

            var enter = state.Update(expectedInside: false, otherInside: true, deltaTime: 1f);
            var stay = state.Update(expectedInside: false, otherInside: true, deltaTime: 1f);
            state.Update(expectedInside: false, otherInside: false, deltaTime: 0.1f);
            var reenter = state.Update(expectedInside: false, otherInside: true, deltaTime: 1f);

            Assert.AreEqual(ScanResult.WrongItem, enter);
            Assert.AreEqual(ScanResult.None, stay);
            Assert.AreEqual(ScanResult.WrongItem, reenter);
            Assert.AreEqual(0, state.NextIndex);
        }

        [Test]
        public void ExpectedAndOtherTogether_ExpectedStillScans_WithoutWrongReport()
        {
            var state = Create();

            var first = state.Update(expectedInside: true, otherInside: true, deltaTime: 0.3f);
            var second = state.Update(expectedInside: true, otherInside: true, deltaTime: 0.3f);

            Assert.AreEqual(ScanResult.None, first);
            Assert.AreEqual(ScanResult.Scanned, second);
        }

        [Test]
        public void AllItemsScanned_IsComplete_AndIgnoresFurtherInput()
        {
            var state = Create();
            state.Update(true, false, 0.5f);

            var last = state.Update(true, false, 0.5f);
            var afterComplete = state.Update(true, true, 1f);

            Assert.AreEqual(ScanResult.Scanned, last);
            Assert.IsTrue(state.IsComplete);
            Assert.AreEqual(ScanResult.None, afterComplete);
        }

        [Test]
        public void ResetForRecovery_MidSequence_KeepsScannedProgress()
        {
            var state = Create();
            state.Update(true, false, 0.5f);
            state.Update(true, false, 0.3f);

            state.ResetForRecovery();
            var result = state.Update(true, false, 0.3f);

            Assert.AreEqual(1, state.NextIndex);
            Assert.AreEqual(ScanResult.None, result);
        }

        [Test]
        public void ResetForRecovery_AfterComplete_StartsOver()
        {
            var state = Create();
            state.Update(true, false, 0.5f);
            state.Update(true, false, 0.5f);

            state.ResetForRecovery();

            Assert.IsFalse(state.IsComplete);
            Assert.AreEqual(0, state.NextIndex);
        }

        [Test]
        public void JumpTo_RealignsToScenarioStep_EvenIfScannedEarly()
        {
            var state = Create();
            state.Update(true, false, 0.5f); // Step 전에 미리 스캔됨 → 다음 차례로 앞서 나감

            state.JumpTo(0);
            var result = state.Update(true, false, 0.5f);

            Assert.AreEqual(ScanResult.Scanned, result);
            Assert.AreEqual(1, state.NextIndex);
        }

        [Test]
        public void JumpTo_OutOfRange_IsIgnored()
        {
            var state = Create();

            state.JumpTo(5);
            state.JumpTo(-1);

            Assert.AreEqual(0, state.NextIndex);
        }
    }
}
