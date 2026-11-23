// 현재 Step의 목표 물체를 깜빡여 강조하고, 약 5초간 진행이 없으면 더 강하게 강조하는 컴포넌트(Material 복제 없이 색만 바꾼다)
using System.Collections.Generic;
using UnityEngine;

namespace GatePassVR.Guidance
{
    public class HighlightController : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

        private readonly HighlightState state = new HighlightState();
        private readonly List<Entry> entries = new List<Entry>();
        private MaterialPropertyBlock block;
        private Transform target;

        private class Entry
        {
            public Renderer renderer;
            public int colorId;
            public Color baseColor;    // 강조 계산의 시작 색이자 되돌릴 색
            public Color lastWritten;  // 마지막으로 이 컴포넌트가 넣었거나 처음 읽은 색
        }

        public Transform Target => target;
        public HighlightLevel Level => state.Level;

        // ScenarioManager가 Step에 들어갈 때 부른다. null이면 강조를 끈다.
        public void SetTarget(Transform newTarget)
        {
            Restore();
            target = newTarget;
            state.SetHasTarget(target != null);

            if (target == null)
            {
                return;
            }

            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }

            foreach (var r in target.GetComponentsInChildren<Renderer>())
            {
                var mat = r.sharedMaterial;
                int id = mat != null && mat.HasProperty(BaseColorId) ? BaseColorId
                    : mat != null && mat.HasProperty(ColorId) ? ColorId : -1;
                if (id < 0)
                {
                    continue;
                }

                // 처음 읽은 색을 마지막으로 알고 있는 색으로 둬야, 첫 강조 전에 다른 컴포넌트가 바꾼 색도 알아챈다.
                var current = CurrentColor(r, id, mat);
                entries.Add(new Entry { renderer = r, colorId = id, baseColor = current, lastWritten = current });
            }
        }

        // GuideManager.onNoProgressShort(약 5초 무반응)에 연결한다.
        public void Escalate()
        {
            state.Escalate();
        }

        private void Update()
        {
            if (entries.Count == 0)
            {
                return;
            }

            float amount = state.Evaluate(Time.time);
            foreach (var e in entries)
            {
                if (e.renderer == null)
                {
                    continue;
                }

                AdoptExternalChange(e);
                var c = Color.Lerp(e.baseColor, highlightColor, amount);
                c.a = e.baseColor.a; // 투명 판(Zone)의 투명도는 유지한다.
                Write(e, c);
            }
        }

        private void OnDisable()
        {
            Restore();
            state.SetHasTarget(false);
            target = null;
        }

        // 강조 전 색으로 되돌린다. 그 사이 다른 컴포넌트(예: PlacementZone 성공 색)가 색을 바꿨으면 그 색을 남긴다.
        private void Restore()
        {
            foreach (var e in entries)
            {
                if (e.renderer == null)
                {
                    continue;
                }

                AdoptExternalChange(e);
                Write(e, e.baseColor);
            }

            entries.Clear();
        }

        // 현재 색이 마지막으로 넣은 색과 다르면 다른 컴포넌트가 바꾼 것이므로 새 기준 색으로 삼는다.
        private void AdoptExternalChange(Entry e)
        {
            var current = CurrentColor(e.renderer, e.colorId, e.renderer.sharedMaterial);
            if (!Approximately(current, e.lastWritten))
            {
                e.baseColor = current;
            }
        }

        private void Write(Entry e, Color c)
        {
            e.renderer.GetPropertyBlock(block);
            block.SetColor(e.colorId, c);
            e.renderer.SetPropertyBlock(block);
            e.lastWritten = c;
        }

        private Color CurrentColor(Renderer r, int id, Material mat)
        {
            r.GetPropertyBlock(block);
            if (block.HasColor(id))
            {
                return block.GetColor(id);
            }

            return mat != null ? mat.GetColor(id) : Color.white;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.002f && Mathf.Abs(a.g - b.g) < 0.002f
                && Mathf.Abs(a.b - b.b) < 0.002f && Mathf.Abs(a.a - b.a) < 0.002f;
        }
    }
}
