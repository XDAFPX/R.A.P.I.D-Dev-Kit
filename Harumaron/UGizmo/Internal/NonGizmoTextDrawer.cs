using System.Collections.Generic;
using UnityEngine;

namespace RapidLib.Harumaron.UGizmo.Internal
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NonGizmoTextDrawer : MonoBehaviour
    {
        private class LabelEntry
        {
            public Vector3 worldPos;
            public string text;
            public float expireTime;
            public Color color;
            public int fontSize;
        }

        private static NonGizmoTextDrawer _instance;
        private readonly List<LabelEntry> _labels = new();
        private static readonly GUIStyle _style = new();
        private static bool _styleInit;

        // ── Public API ───────────────────────────────────────────────

        public static void Draw(Vector3 worldPos, string text, float duration, Color color, int fontSize = 14)
        {
            EnsureInstance();
            _instance._labels.Add(new LabelEntry
            {
                worldPos = worldPos,
                text = text,
                expireTime = Time.time + duration,
                color = color,
                fontSize = fontSize
            });
        }


        // ── Internals ────────────────────────────────────────────────

        private static void EnsureInstance()
        {
            if (_instance != null) return;
            var go = new GameObject("[NonGizmoTextDrawer]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<NonGizmoTextDrawer>();
        }

        private void OnGUI()
        {
            InitStyle();

            float now = Time.time;

            for (int i = _labels.Count - 1; i >= 0; i--)
            {
                var entry = _labels[i];

                if (now >= entry.expireTime)
                {
                    _labels.RemoveAt(i);
                    continue;
                }

                Vector3 screenPos = Camera.main.WorldToScreenPoint(entry.worldPos);
                if (screenPos.z < 0) continue;

                float guiX = screenPos.x + 12f;
                float guiY = Screen.height - screenPos.y - 20f;

                float alpha = Mathf.Clamp01((entry.expireTime - now) / 0.5f);
                _style.fontSize = entry.fontSize;
                _style.normal.textColor = new Color(entry.color.r, entry.color.g, entry.color.b, alpha);

                GUI.Label(new Rect(guiX, guiY, 400, 100), entry.text, _style);
            }
        }

        private static void InitStyle()
        {
            if (_styleInit) return;
            _style.fontStyle = FontStyle.Bold;
            _styleInit = true;
        }
    }
}