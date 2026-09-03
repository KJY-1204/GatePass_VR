// Point & Hold 게이지의 순수 상태 로직. MonoBehaviour와 분리해 EditMode 테스트가 가능하다.
using UnityEngine;

namespace GatePassVR.Interaction
{
    public class HoldGaugeState
    {
        private readonly float holdDuration;
        private readonly float decayRate;

        public float Progress { get; private set; }
        public bool IsHovering { get; private set; }
        public bool IsCompleted { get; private set; }

        public HoldGaugeState(float holdDuration, float decayRate)
        {
            this.holdDuration = Mathf.Max(0.01f, holdDuration);
            this.decayRate = Mathf.Max(0f, decayRate);
        }

        public void SetHovering(bool hovering)
        {
            IsHovering = hovering;
        }

        // deltaTime만큼 게이지를 진행시킨다. 이번 호출에서 막 100%를 채웠으면 true를 반환한다.
        public bool Tick(float deltaTime)
        {
            if (IsCompleted)
            {
                return false;
            }

            if (IsHovering)
            {
                Progress += deltaTime / holdDuration;
            }
            else
            {
                Progress -= deltaTime * decayRate / holdDuration;
            }

            Progress = Mathf.Clamp01(Progress);

            if (Progress >= 1f)
            {
                IsCompleted = true;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            Progress = 0f;
            IsHovering = false;
            IsCompleted = false;
        }
    }
}
