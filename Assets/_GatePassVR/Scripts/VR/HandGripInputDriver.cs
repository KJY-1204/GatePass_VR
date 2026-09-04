// 컨트롤러의 Grip/Trigger 입력값을 읽어 FingerCurlAnimator를 구동한다
using UnityEngine;
using UnityEngine.XR;

namespace GatePassVR.VR
{
    public class HandGripInputDriver : MonoBehaviour
    {
        [SerializeField] private XRNode handNode = XRNode.RightHand;
        [SerializeField] private FingerCurlAnimator curlAnimator;

        private void Update()
        {
            if (curlAnimator == null)
            {
                return;
            }

            var device = InputDevices.GetDeviceAtXRNode(handNode);
            if (!device.isValid)
            {
                return;
            }

            device.TryGetFeatureValue(CommonUsages.grip, out float grip);
            device.TryGetFeatureValue(CommonUsages.trigger, out float trigger);

            curlAnimator.SetCurl(Mathf.Max(grip, trigger));
        }
    }
}
