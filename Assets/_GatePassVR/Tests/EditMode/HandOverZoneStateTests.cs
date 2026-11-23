// HandOverZoneState의 건네기 1회 성공·틀린 물건·반환·돌려받기·Reset 동작을 검증하는 EditMode 테스트
using GatePassVR.Interaction;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class HandOverZoneStateTests
    {
        [Test]
        public void AcceptedItemInside_HandsOverOnce()
        {
            var state = new HandOverZoneState();

            var first = state.Update(acceptedInside: true, wrongInside: false);
            var second = state.Update(acceptedInside: true, wrongInside: false);

            Assert.AreEqual(HandOverResult.HandedOver, first);
            Assert.AreEqual(HandOverResult.None, second);
            Assert.AreEqual(HandOverPhase.HandedOver, state.Phase);
        }

        [Test]
        public void WrongItemInside_ReportsOncePerEntry_AndNeverHandsOver()
        {
            var state = new HandOverZoneState();

            var enter = state.Update(acceptedInside: false, wrongInside: true);
            var stay = state.Update(acceptedInside: false, wrongInside: true);
            state.Update(acceptedInside: false, wrongInside: false);
            var reenter = state.Update(acceptedInside: false, wrongInside: true);

            Assert.AreEqual(HandOverResult.WrongObject, enter);
            Assert.AreEqual(HandOverResult.None, stay);
            Assert.AreEqual(HandOverResult.WrongObject, reenter);
            Assert.AreEqual(HandOverPhase.Waiting, state.Phase);
        }

        [Test]
        public void AcceptedAndWrongTogether_AcceptedWins()
        {
            var state = new HandOverZoneState();

            var result = state.Update(acceptedInside: true, wrongInside: true);

            Assert.AreEqual(HandOverResult.HandedOver, result);
        }

        [Test]
        public void TryTake_BeforeReturn_IsIgnored()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);

            Assert.IsFalse(state.TryTake());
            Assert.AreEqual(HandOverPhase.HandedOver, state.Phase);
        }

        [Test]
        public void TryTake_AfterReturn_SucceedsOnce()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);

            bool returned = state.MarkReturned();
            bool firstTake = state.TryTake();
            bool secondTake = state.TryTake();

            Assert.IsTrue(returned);
            Assert.IsTrue(firstTake);
            Assert.IsFalse(secondTake);
            Assert.AreEqual(HandOverPhase.Taken, state.Phase);
        }

        [Test]
        public void MarkReturned_BeforeHandOver_IsIgnored()
        {
            var state = new HandOverZoneState();

            Assert.IsFalse(state.MarkReturned());
            Assert.AreEqual(HandOverPhase.Waiting, state.Phase);
        }

        [Test]
        public void AfterReturn_ItemInsideAgain_DoesNotHandOverAgain()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);
            state.MarkReturned();

            var result = state.Update(acceptedInside: true, wrongInside: false);

            Assert.AreEqual(HandOverResult.None, result);
        }

        [Test]
        public void Reset_ReturnsToWaiting_AndAllowsHandOverAgain()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);
            state.MarkReturned();
            state.TryTake();

            state.Reset();
            var result = state.Update(acceptedInside: true, wrongInside: false);

            Assert.AreEqual(HandOverResult.HandedOver, result);
        }

        [Test]
        public void ResetForRecovery_BeforeHandOver_GoesBackToWaiting()
        {
            var state = new HandOverZoneState();

            bool mustReturn = state.ResetForRecovery();

            Assert.IsFalse(mustReturn);
            Assert.AreEqual(HandOverPhase.Waiting, state.Phase);
        }

        [Test]
        public void ResetForRecovery_WhileWaitingForReturn_ReturnsImmediately()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);

            bool mustReturn = state.ResetForRecovery();

            Assert.IsTrue(mustReturn);
            Assert.AreEqual(HandOverPhase.Returned, state.Phase);
            Assert.IsTrue(state.TryTake());
        }

        [Test]
        public void ResetForRecovery_AfterReturned_StaysReturned()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);
            state.MarkReturned();

            bool mustReturn = state.ResetForRecovery();

            Assert.IsFalse(mustReturn);
            Assert.AreEqual(HandOverPhase.Returned, state.Phase);
        }

        [Test]
        public void ResetForRecovery_AfterTaken_GoesBackToWaiting()
        {
            var state = new HandOverZoneState();
            state.Update(acceptedInside: true, wrongInside: false);
            state.MarkReturned();
            state.TryTake();

            bool mustReturn = state.ResetForRecovery();

            Assert.IsFalse(mustReturn);
            Assert.AreEqual(HandOverPhase.Waiting, state.Phase);
        }
    }
}
