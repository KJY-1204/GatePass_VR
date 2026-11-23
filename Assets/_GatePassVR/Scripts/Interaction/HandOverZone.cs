// 여권 등 지정 물체가 전달 영역에 들어오면 건네기 성공(1회) 처리하고, 설정 시 잠시 뒤 돌려주어 다시 집게 하는 Hand-over 컴포넌트(다음 Step은 ScenarioManager가 정한다)
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Interaction
{
    [RequireComponent(typeof(BoxCollider))]
    public class HandOverZone : MonoBehaviour
    {
        public const string ReturnedSuffix = "_Returned";

        [SerializeField] private string zoneId;
        [Tooltip("이 영역이 받는 물건. 여기 없는 집는 물건이 들어오면 onWrongObject가 발생한다.")]
        [SerializeField] private XRGrabInteractable[] acceptedItems = new XRGrabInteractable[0];
        [Tooltip("영역 판 위로 물건을 찾는 높이(m)")]
        [SerializeField] private float detectHeight = 0.4f;
        [Tooltip("건넨 물건을 둘 자리(직원·심사관 쪽). 없으면 그 자리에 고정한다.")]
        [SerializeField] private Transform receivePoint;
        [Tooltip("돌려줄 자리. 없으면 돌려주지 않는다.")]
        [SerializeField] private Transform returnPoint;
        [SerializeField] private float returnDelay = 2f;

        [SerializeField] private UnityEvent<HandOverZone> onHandedOver;
        [SerializeField] private UnityEvent<HandOverZone> onWrongObject;
        [SerializeField] private UnityEvent<HandOverZone> onReturned;
        [SerializeField] private UnityEvent<HandOverZone> onReturnTaken;

        private readonly Collider[] overlapBuffer = new Collider[32];
        private readonly HandOverZoneState state = new HandOverZoneState();

        private BoxCollider zoneCollider;
        private XRGrabInteractable heldItem;
        private bool heldWasKinematic;
        private Coroutine returnRoutine;

        public string ZoneId => zoneId;
        public string ReturnTakenId => zoneId + ReturnedSuffix;
        public HandOverPhase Phase => state.Phase;
        public XRGrabInteractable HandedItem => heldItem;

        private void Awake()
        {
            zoneCollider = GetComponent<BoxCollider>();
            zoneCollider.isTrigger = true;
        }

        private void Update()
        {
            if (state.Phase != HandOverPhase.Waiting)
            {
                return;
            }

            XRGrabInteractable accepted = null;
            bool wrongInside = false;
            FindItemsInside(ref accepted, ref wrongInside);

            switch (state.Update(accepted != null, wrongInside))
            {
                case HandOverResult.HandedOver:
                    Receive(accepted);
                    onHandedOver?.Invoke(this);
                    if (returnPoint != null)
                    {
                        returnRoutine = StartCoroutine(ReturnAfterDelay());
                    }
                    break;
                case HandOverResult.WrongObject:
                    onWrongObject?.Invoke(this);
                    break;
            }
        }

        // Reset 버튼용 복구. 건네기 전·돌려받은 뒤면 처음 상태로, 반환을 기다리는 중이면 바로 돌려줘서 집을 수 있게 한다.
        // 물건 위치 복구는 ResetController가 한다(돌려준 물건은 원위치에서 집어도 돌려받기 성공).
        public void ResetZone()
        {
            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
                returnRoutine = null;
            }

            var phaseBefore = state.Phase;
            if (state.ResetForRecovery())
            {
                Release(heldItem);
                heldItem.selectEntered.AddListener(OnReturnedItemSelected);
                onReturned?.Invoke(this);
                return;
            }

            if (phaseBefore == HandOverPhase.Returned)
            {
                return;
            }

            if (heldItem != null)
            {
                heldItem.selectEntered.RemoveListener(OnReturnedItemSelected);
                Release(heldItem);
                heldItem = null;
            }
        }

        private void Receive(XRGrabInteractable item)
        {
            heldItem = item;

            // 손에 든 채로 건넸으면 먼저 놓게 해야 직원 쪽으로 옮길 수 있다.
            if (item.isSelected && item.interactionManager != null)
            {
                item.interactionManager.CancelInteractableSelection((IXRSelectInteractable)item);
            }

            var body = item.GetComponent<Rigidbody>();
            if (body != null)
            {
                heldWasKinematic = body.isKinematic;
                body.isKinematic = true;
            }

            item.enabled = false;

            if (receivePoint != null)
            {
                MoveItem(item, receivePoint);
            }
        }

        private IEnumerator ReturnAfterDelay()
        {
            yield return new WaitForSeconds(returnDelay);
            returnRoutine = null;

            if (heldItem == null || !state.MarkReturned())
            {
                yield break;
            }

            MoveItem(heldItem, returnPoint);
            Release(heldItem);
            heldItem.selectEntered.AddListener(OnReturnedItemSelected);
            onReturned?.Invoke(this);
        }

        private void OnReturnedItemSelected(SelectEnterEventArgs args)
        {
            if (!state.TryTake())
            {
                return;
            }

            heldItem.selectEntered.RemoveListener(OnReturnedItemSelected);
            heldItem = null;
            onReturnTaken?.Invoke(this);
        }

        private void Release(XRGrabInteractable item)
        {
            var body = item.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = heldWasKinematic;
            }

            item.enabled = true;
        }

        private static void MoveItem(XRGrabInteractable item, Transform point)
        {
            item.transform.SetPositionAndRotation(point.position, point.rotation);
            var body = item.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = point.position;
                body.rotation = point.rotation;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        // 판 아래 5cm부터 판 위 detectHeight까지의 상자(가로·세로는 판 크기) 안에 들어온 집는 물건을 찾는다. 손에 든 물건도 센다.
        private void FindItemsInside(ref XRGrabInteractable accepted, ref bool wrongInside)
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

                if (System.Array.IndexOf(acceptedItems, item) >= 0)
                {
                    accepted = item;
                }
                else
                {
                    wrongInside = true;
                }
            }
        }
    }
}
