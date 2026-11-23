// 시나리오의 현재 Step 위치와 성공 ID 검증(1회 진행·완료·재시작)만 담은 순수 상태 로직. EditMode 테스트가 가능하다.
using System.Collections.Generic;

namespace GatePassVR.Core
{
    public class ScenarioProgress
    {
        private readonly IReadOnlyList<string> successIds;

        public int CurrentIndex { get; private set; }
        public bool IsFinished => CurrentIndex >= successIds.Count;

        public ScenarioProgress(IReadOnlyList<string> successIds)
        {
            this.successIds = successIds;
        }

        // 현재 Step이 기다리는 ID일 때만 다음 Step으로 넘어간다. 넘어갔으면 true.
        // 넘어간 뒤에는 다음 Step의 ID를 기다리므로 같은 성공이 두 번 와도 한 번만 진행된다.
        public bool TryAdvance(string successId)
        {
            if (IsFinished || string.IsNullOrEmpty(successId) || successIds[CurrentIndex] != successId)
            {
                return false;
            }

            CurrentIndex++;
            return true;
        }

        public void Restart()
        {
            CurrentIndex = 0;
        }
    }
}
