// World Space UI를 카메라 앞 고정 위치/각도로 따라다니게 하는 범용 추종 컴포넌트
using UnityEngine;

namespace GatePassVR.UI
{
    public class GuideUIFollow : MonoBehaviour
    {
        [SerializeField] private Transform target; // 비워두면 Camera.main을 자동으로 찾는다
        [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.1f, 1.3f);

        private Transform followTarget;

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                followTarget = target != null ? target : (Camera.main != null ? Camera.main.transform : null);

                if (followTarget == null)
                {
                    return;
                }
            }

            transform.SetPositionAndRotation(
                followTarget.TransformPoint(localOffset),
                followTarget.rotation);
        }
    }
}
