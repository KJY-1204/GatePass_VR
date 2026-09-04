// Point & Hold 진행률(0~1)을 오브젝트 색상 변화로 보여주는 임시 시각 피드백
using UnityEngine;

namespace GatePassVR.Interaction
{
    [RequireComponent(typeof(Renderer))]
    public class HoldProgressVisual : MonoBehaviour
    {
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color completeColor = Color.green;

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        // PointAndHoldTarget.onProgressChanged(float)에 연결해서 사용한다.
        public void SetProgress(float progress)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", Color.Lerp(idleColor, completeColor, progress));
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
