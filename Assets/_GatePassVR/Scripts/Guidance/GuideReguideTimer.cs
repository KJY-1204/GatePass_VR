// 안내 후 무반응 시간을 추적해 5초/10초 재안내 시점을 판단하는 순수 상태 로직. MonoBehaviour와 분리해 EditMode 테스트가 가능하다.
using UnityEngine;

namespace GatePassVR.Guidance
{
    public class GuideReguideTimer
    {
        private readonly float shortThreshold;
        private readonly float longThreshold;

        public float ElapsedSinceProgress { get; private set; }
        public bool ShortFired { get; private set; }
        public bool LongFired { get; private set; }

        public GuideReguideTimer(float shortThreshold, float longThreshold)
        {
            this.shortThreshold = Mathf.Max(0f, shortThreshold);
            this.longThreshold = Mathf.Max(this.shortThreshold, longThreshold);
        }

        public void Reset()
        {
            ElapsedSinceProgress = 0f;
            ShortFired = false;
            LongFired = false;
        }

        // deltaTime만큼 시간을 진행시키고, 이번 호출에서 막 넘긴 임계값을 알려준다.
        public (bool shortJustFired, bool longJustFired) Tick(float deltaTime)
        {
            ElapsedSinceProgress += deltaTime;

            bool shortJustFired = false;
            bool longJustFired = false;

            if (!ShortFired && ElapsedSinceProgress >= shortThreshold)
            {
                ShortFired = true;
                shortJustFired = true;
            }

            if (!LongFired && ElapsedSinceProgress >= longThreshold)
            {
                LongFired = true;
                longJustFired = true;
            }

            return (shortJustFired, longJustFired);
        }
    }
}
