// Point & Hold 진행률(0~1)을 시계 방향으로 차오르는 2D 도넛 게이지로 보여준다
using UnityEngine;
using UnityEngine.UI;

namespace GatePassVR.Interaction
{
    [RequireComponent(typeof(Image))]
    public class RadialGaugeVisual : MonoBehaviour
    {
        private static Sprite ringSpriteCache;

        [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color completeColor = new Color(0.2f, 1f, 0.4f, 1f);

        private Image fillImage;

        private void Awake()
        {
            fillImage = GetComponent<Image>();
            fillImage.sprite = GetOrCreateRingSprite();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = true;
            fillImage.fillAmount = 0f;
            fillImage.color = idleColor;
        }

        // PointAndHoldTarget.onProgressChanged(float)에 연결해서 사용한다.
        public void SetProgress(float progress)
        {
            fillImage.fillAmount = progress;
            fillImage.color = Color.Lerp(idleColor, completeColor, progress);
        }

        // 코드로 생성한 임시 도넛 스프라이트. 실제 아트가 들어오면 Inspector에서 sprite를 직접 지정해 대체하면 된다.
        private static Sprite GetOrCreateRingSprite()
        {
            if (ringSpriteCache != null)
            {
                return ringSpriteCache;
            }

            const int size = 256;
            const float outerRadius = 0.48f;
            const float innerRadius = 0.34f;
            const float halfSize = size / 2f;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(halfSize, halfSize)) / halfSize;
                    bool inRing = dist <= outerRadius && dist >= innerRadius;
                    pixels[y * size + x] = inRing ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            ringSpriteCache = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return ringSpriteCache;
        }
    }
}
