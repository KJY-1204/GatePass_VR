// 한 Step의 안내 문구·음성, 화살표 목표, 성공 조건 ID, 성공 시 이동 목적지를 Inspector에서 설정하는 데이터
using System;
using UnityEngine;

namespace GatePassVR.Core
{
    [Serializable]
    public class ScenarioStep
    {
        [Tooltip("문서·로그용 이름 (예: S1-1, guide-texts.md ID)")]
        public string stepId;
        [Tooltip("이 Step을 성공시키는 상호작용 ID (PointAndHoldTarget.targetId, PlacementZone.zoneId 등)")]
        public string successId;
        [TextArea] public string guideMain;
        public string guideHint;
        public AudioClip guideVoice;
        [Tooltip("화살표가 가리킬 지금 상호작용해야 할 대상")]
        public Transform goalTarget;
        [Tooltip("성공하면 Fade로 이동할 위치. 없으면 이동하지 않는다.")]
        public Transform moveDestination;
    }
}
