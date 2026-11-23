// 여권·탑승권처럼 정해진 순서의 물건을 스캔 영역에 잠시 대면 차례대로 스캔 성공을 알리는 Scanner 컴포넌트(다음 Step은 ScenarioManager가 정한다)
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Interaction
{
    [RequireComponent(typeof(BoxCollider))]
    public class ScannerZone : MonoBehaviour
    {
        [Serializable]
        public class ScanEntry
        {
            public XRGrabInteractable item;
            [Tooltip("이 물건을 스캔했을 때 ScenarioManager에 보낼 성공 ID")]
            public string scanId;
        }

        [Tooltip("스캔 순서. 앞의 물건을 스캔해야 다음 물건을 받는다.")]
        [SerializeField] private ScanEntry[] sequence = new ScanEntry[0];
        [Tooltip("물건이 영역에 이 시간(초) 동안 있으면 스캔 성공")]
        [SerializeField] private float scanTime = 0.5f;
        [Tooltip("영역 판 위로 물건을 찾는 높이(m)")]
        [SerializeField] private float detectHeight = 0.3f;

        [SerializeField] private UnityEvent<ScannerZone> onScanned;
        [SerializeField] private UnityEvent<ScannerZone> onWrongItem;

        private readonly Collider[] overlapBuffer = new Collider[32];

        private BoxCollider zoneCollider;
        private ScannerZoneState state;

        public string LastScanId { get; private set; }
        public int NextIndex => state?.NextIndex ?? 0;
        public bool IsComplete => state != null && state.IsComplete;

        private void Awake()
        {
            zoneCollider = GetComponent<BoxCollider>();
            zoneCollider.isTrigger = true;
            state = new ScannerZoneState(sequence.Length, scanTime);
        }

        private void Update()
        {
            if (state.IsComplete)
            {
                return;
            }

            var expected = sequence[state.NextIndex].item;
            bool expectedInside = false, otherInside = false;
            FindItemsInside(expected, ref expectedInside, ref otherInside);

            switch (state.Update(expectedInside, otherInside, Time.deltaTime))
            {
                case ScanResult.Scanned:
                    LastScanId = sequence[state.NextIndex - 1].scanId;
                    onScanned?.Invoke(this);
                    break;
                case ScanResult.WrongItem:
                    onWrongItem?.Invoke(this);
                    break;
            }
        }

        // ScenarioManager.onStepEnteredId에 연결한다. 현재 Step이 이 스캐너의 scanId를 기다리면 그 차례로 맞춘다.
        // Step 전에 물건이 미리 스캔돼 차례가 앞서 나가도 Step에 들어오면 다시 맞춰져 진행이 막히지 않는다.
        public void AlignToScanId(string scanId)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                if (sequence[i].scanId == scanId)
                {
                    state?.JumpTo(i);
                    return;
                }
            }
        }

        // Reset 버튼용 복구. 진행 중이면 스캔한 진행을 유지하고, 전부 마쳤으면 처음부터 다시 스캔하게 한다.
        public void ResetZone()
        {
            state?.ResetForRecovery();
        }

        private bool IsAlreadyScanned(XRGrabInteractable item)
        {
            for (int i = 0; i < state.NextIndex && i < sequence.Length; i++)
            {
                if (sequence[i].item == item)
                {
                    return true;
                }
            }

            return false;
        }

        // 판 아래 5cm부터 판 위 detectHeight까지의 상자(가로·세로는 판 크기) 안에 들어온 집는 물건을 찾는다. 손에 든 물건도 센다.
        private void FindItemsInside(XRGrabInteractable expected, ref bool expectedInside, ref bool otherInside)
        {
            var t = transform;
            var half = Vector3.Scale(zoneCollider.size, t.lossyScale) / 2f;
            float bottom = -half.y - 0.05f;
            var center = t.TransformPoint(zoneCollider.center) + t.up * ((bottom + detectHeight) / 2f);
            half.y = (detectHeight - bottom) / 2f;

            int count = Physics.OverlapBoxNonAlloc(center, half, overlapBuffer, t.rotation, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var body = overlapBuffer[i].attachedRigidbody;
                var item = body != null ? body.GetComponent<XRGrabInteractable>() : null;
                if (item == null || !item.enabled)
                {
                    continue;
                }

                if (item == expected)
                {
                    expectedInside = true;
                }
                else if (!IsAlreadyScanned(item))
                {
                    // 방금 스캔한 물건이 아직 영역에 남아 있는 것은 틀린 물건이 아니다(성공음 뒤에 재시도음이 나지 않게).
                    otherInside = true;
                }
            }
        }
    }
}
