// 목표 강조의 순수 상태 로직(목표 유무, 5초 무반응 시 강화, 시간에 따른 깜빡임 세기). EditMode 테스트가 가능하다.
using UnityEngine;

namespace GatePassVR.Guidance
{
    public enum HighlightLevel
    {
        Off,
        Normal,  // 현재 목표를 은은하게 깜빡인다.
        Strong,  // 약 5초간 진행이 없을 때 더 밝고 빠르게 깜빡인다.
    }

    public class HighlightState
    {
        private const float NormalAmount = 0.35f;
        private const float NormalFrequency = 1f;
        private const float StrongAmount = 0.8f;
        private const float StrongFrequency = 2.5f;

        public HighlightLevel Level { get; private set; }

        // 새 목표가 생기면 Normal로 시작하고, 목표가 없으면 끈다.
        public void SetHasTarget(bool hasTarget)
        {
            Level = hasTarget ? HighlightLevel.Normal : HighlightLevel.Off;
        }

        // 무반응 신호. 목표가 있을 때만 강하게 바꾼다.
        public void Escalate()
        {
            if (Level != HighlightLevel.Off)
            {
                Level = HighlightLevel.Strong;
            }
        }

        // 0~1 사이 강조 세기(원래 색 → 강조 색 보간 비율).
        public float Evaluate(float time)
        {
            switch (Level)
            {
                case HighlightLevel.Normal:
                    return Pulse(time, NormalAmount, NormalFrequency);
                case HighlightLevel.Strong:
                    return Pulse(time, StrongAmount, StrongFrequency);
                default:
                    return 0f;
            }
        }

        private static float Pulse(float time, float amount, float frequency)
        {
            return amount * (0.5f + 0.5f * Mathf.Sin(time * frequency * 2f * Mathf.PI));
        }
    }
}
