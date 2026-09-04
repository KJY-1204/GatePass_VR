// 전용 그립 애니메이션이 없는 손 모델을 위해, 손가락 관절을 로컬 X축으로 굽혀 그립 포즈를 대신 만든다
using UnityEngine;

namespace GatePassVR.VR
{
    public class FingerCurlAnimator : MonoBehaviour
    {
        [System.Serializable]
        private struct Joint
        {
            public Transform bone;
            public float curlAngle;
        }

        [SerializeField] private Joint[] joints;

        private Quaternion[] openRotations;

        private void Awake()
        {
            openRotations = new Quaternion[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i].bone != null)
                {
                    openRotations[i] = joints[i].bone.localRotation;
                }
            }
        }

        // 0(편 손)~1(주먹) 사이 값으로 모든 손가락 관절을 함께 굽힌다.
        public void SetCurl(float curl)
        {
            curl = Mathf.Clamp01(curl);

            for (int i = 0; i < joints.Length; i++)
            {
                var bone = joints[i].bone;
                if (bone == null)
                {
                    continue;
                }

                bone.localRotation = openRotations[i] * Quaternion.AngleAxis(joints[i].curlAngle * curl, Vector3.right);
            }
        }
    }
}
