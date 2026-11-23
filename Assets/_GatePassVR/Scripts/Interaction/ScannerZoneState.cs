// Scanner의 순수 판정 로직(정해진 순서, 머무름 시간, 1회 스캔, 틀린 물건, Reset 복구). MonoBehaviour와 분리해 EditMode 테스트가 가능하다.
namespace GatePassVR.Interaction
{
    public enum ScanResult
    {
        None,
        Scanned,    // 지금 차례 물건이 scanTime 동안 영역에 있었다(물건마다 한 번).
        WrongItem,  // 차례가 아닌 물건이 영역에 들어왔다(들어올 때마다 한 번).
    }

    public class ScannerZoneState
    {
        private readonly int itemCount;
        private readonly float scanTime;
        private float dwell;
        private bool wrongReported;

        public int NextIndex { get; private set; }
        public bool IsComplete => NextIndex >= itemCount;

        public ScannerZoneState(int itemCount, float scanTime)
        {
            this.itemCount = itemCount;
            this.scanTime = scanTime < 0f ? 0f : scanTime;
        }

        // 매 프레임 영역 안 상태를 넣는다. 지금 차례 물건이 있으면 다른 물건이 같이 있어도 스캔을 계속한다.
        public ScanResult Update(bool expectedInside, bool otherInside, float deltaTime)
        {
            if (IsComplete)
            {
                return ScanResult.None;
            }

            if (expectedInside)
            {
                dwell += deltaTime < 0f ? 0f : deltaTime;
                if (dwell >= scanTime)
                {
                    dwell = 0f;
                    NextIndex++;
                    return ScanResult.Scanned;
                }

                return ScanResult.None;
            }

            dwell = 0f;

            if (!otherInside)
            {
                wrongReported = false;
                return ScanResult.None;
            }

            if (wrongReported)
            {
                return ScanResult.None;
            }

            wrongReported = true;
            return ScanResult.WrongItem;
        }

        // 시나리오가 기다리는 차례로 맞춘다. Step 전에 미리 스캔돼 앞서 나간 경우 다시 맞추기 위해 쓴다.
        public void JumpTo(int index)
        {
            if (index < 0 || index >= itemCount)
            {
                return;
            }

            NextIndex = index;
            dwell = 0f;
            wrongReported = false;
        }

        // Reset 버튼용 복구. 진행 중에 처음으로 되돌리면 Step과 차례가 어긋나 진행이 막히므로 스캔한 진행은 유지한다.
        // 전부 마친 뒤에는 처음부터 다시 스캔할 수 있게 한다.
        public void ResetForRecovery()
        {
            if (IsComplete)
            {
                NextIndex = 0;
            }

            dwell = 0f;
            wrongReported = false;
        }
    }
}
