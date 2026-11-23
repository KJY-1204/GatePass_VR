// Hand-over의 순수 판정 로직(건네기 1회, 틀린 물건, 반환, 돌려받기, Reset). MonoBehaviour와 분리해 EditMode 테스트가 가능하다.
namespace GatePassVR.Interaction
{
    public enum HandOverPhase
    {
        Waiting,     // 아직 건네지 않았다.
        HandedOver,  // 직원·심사관이 받았다.
        Returned,    // 돌려줘서 다시 집을 수 있다.
        Taken,       // 사용자가 돌려받았다.
    }

    public enum HandOverResult
    {
        None,
        HandedOver,   // 받는 물건이 영역에 들어왔다(한 번만).
        WrongObject,  // 받지 않는 물건이 영역에 들어왔다(들어올 때마다 한 번).
    }

    public class HandOverZoneState
    {
        private bool wrongReported;

        public HandOverPhase Phase { get; private set; }

        // 매 프레임 영역 안 상태를 넣는다. 받는 물건과 틀린 물건이 같이 있으면 받는 물건을 우선한다.
        public HandOverResult Update(bool acceptedInside, bool wrongInside)
        {
            if (Phase != HandOverPhase.Waiting)
            {
                return HandOverResult.None;
            }

            if (acceptedInside)
            {
                Phase = HandOverPhase.HandedOver;
                return HandOverResult.HandedOver;
            }

            if (!wrongInside)
            {
                wrongReported = false;
                return HandOverResult.None;
            }

            if (wrongReported)
            {
                return HandOverResult.None;
            }

            wrongReported = true;
            return HandOverResult.WrongObject;
        }

        // 건넨 물건을 돌려줬다. 건넨 상태에서만 true.
        public bool MarkReturned()
        {
            if (Phase != HandOverPhase.HandedOver)
            {
                return false;
            }

            Phase = HandOverPhase.Returned;
            return true;
        }

        // 돌려준 물건을 사용자가 집었다. 돌려준 상태에서 한 번만 true.
        public bool TryTake()
        {
            if (Phase != HandOverPhase.Returned)
            {
                return false;
            }

            Phase = HandOverPhase.Taken;
            return true;
        }

        // Reset 버튼용 복구. 건넨 뒤 돌려받기 Step에서 처음으로 되돌리면 받을 물건이 없어 진행이 막히므로,
        // 반환을 기다리는 중이면 바로 돌려준 상태로 앞당긴다(true = 지금 물건을 돌려줘야 함).
        // 건네기 전이나 이미 돌려받은 뒤에는 처음 상태로 되돌린다.
        public bool ResetForRecovery()
        {
            switch (Phase)
            {
                case HandOverPhase.HandedOver:
                    Phase = HandOverPhase.Returned;
                    return true;
                case HandOverPhase.Returned:
                    return false;
                default:
                    Reset();
                    return false;
            }
        }

        public void Reset()
        {
            Phase = HandOverPhase.Waiting;
            wrongReported = false;
        }
    }
}
