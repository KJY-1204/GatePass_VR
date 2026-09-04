// Ray로 목표를 가리켜 Hold하면 게이지가 차고, 완료 시 Point & Hold 성공 이벤트를 발생시키는 컴포넌트
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Interaction
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public class PointAndHoldTarget : MonoBehaviour
    {
        [SerializeField] private string targetId;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float decayRate = 2f;

        [SerializeField] private UnityEvent<float> onProgressChanged;
        [SerializeField] private UnityEvent<PointAndHoldTarget> onHoldCompleted;

        private XRBaseInteractable interactable;
        private HoldGaugeState gauge;

        public string TargetId => targetId;
        public float Progress => gauge?.Progress ?? 0f;
        public bool IsCompleted => gauge?.IsCompleted ?? false;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            gauge = new HoldGaugeState(holdDuration, decayRate);
        }

        private void OnEnable()
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }

        private void OnDisable()
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        private void Update()
        {
            bool justCompleted = gauge.Tick(Time.deltaTime);
            onProgressChanged?.Invoke(gauge.Progress);

            if (justCompleted)
            {
                onHoldCompleted?.Invoke(this);
            }
        }

        // 이 Step을 다시 시도할 수 있도록 게이지를 초기 상태로 되돌린다.
        public void ResetHold()
        {
            gauge.Reset();
            onProgressChanged?.Invoke(gauge.Progress);
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            gauge.SetHovering(true);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            gauge.SetHovering(interactable.isHovered);
        }
    }
}
