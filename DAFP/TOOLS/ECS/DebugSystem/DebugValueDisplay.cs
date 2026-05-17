using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public class DebugValueDisplay : ITickable 
    {
        private readonly Canvas canvas;
        private readonly GameObject root;
        private readonly GameObject panel;
        private readonly Dictionary<string, (Func<string>,TextMeshProUGUI)> labels = new();
        private readonly Color color;
        private readonly int fontSize;

        private const int ROW_HEIGHT = 20;
        private const int PANEL_WIDTH = 220;
        private const int PADDING_LEFT = 1;

        public DebugValueDisplay(IEnumerable<DebugValue> values, Color color, int fontSize = 14)
        {
            this.color = color;
            this.fontSize = fontSize;
            root = new GameObject("DebugValueDisplay");
            canvas = setup_canvas();
            panel = setup_panel();
            UpdateValues(values);
        }

        private Canvas setup_canvas()
        {
            var _canvasGo = new GameObject("Canvas");
            _canvasGo.transform.SetParent(root.transform, false);

            var _canvas = _canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;

            _canvasGo.AddComponent<CanvasScaler>();
            _canvasGo.AddComponent<GraphicRaycaster>();

            return _canvas;
        }

        private GameObject setup_panel()
        {
            var _panelGo = new GameObject("Panel");
            _panelGo.transform.SetParent(canvas.transform, false);

            var _rect = _panelGo.AddComponent<RectTransform>();
            _rect.anchorMin = new Vector2(1, 1);
            _rect.anchorMax = new Vector2(1, 1);
            _rect.pivot = new Vector2(1, 1);
            _rect.anchoredPosition = Vector2.zero;
            _rect.sizeDelta = new Vector2(PANEL_WIDTH, 0);

            var _image = _panelGo.AddComponent<Image>();
            _image.color = new Color(0, 0, 0, 0f);

            var _layout = _panelGo.AddComponent<VerticalLayoutGroup>();
            _layout.padding = new RectOffset(PADDING_LEFT, PADDING_LEFT, 4, 4);
            _layout.spacing = 2;
            _layout.childForceExpandWidth = true;
            _layout.childForceExpandHeight = false;

            _panelGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return _panelGo;
        }

        public void UpdateValues(IEnumerable<DebugValue> values)
        {
            var _incoming = values.ToList();
            var _incomingNames = _incoming.Select(v => v.Name).ToHashSet();

            // Remove labels that are no longer in the enumerable
            var _toRemove = labels.Keys.Where(k => !_incomingNames.Contains(k)).ToList();
            foreach (var _name in _toRemove)
            {
                Object.Destroy(labels[_name].Item2.gameObject);
                labels.Remove(_name);
            }

            // Add labels for new values
            foreach (var _value in _incoming.Where(v => !labels.ContainsKey(v.Name)))
                add_row(_value);
        }

        private void add_row(DebugValue value)
        {
            var _row = new GameObject(value.Name);
            _row.transform.SetParent(panel.transform, false);

            var _rect = _row.AddComponent<RectTransform>();
            _rect.sizeDelta = new Vector2(0, ROW_HEIGHT);

            var _text = _row.AddComponent<TextMeshProUGUI>();
            _text.fontSize = fontSize;
            _text.color = color;
            _text.text = format(value.Name, "---");

            labels[value.Name] = (value.Stream, _text);

        }


        public void Tick()
        {
            foreach (var _label in labels)
            {
                _label.Value.Item2.text = format(_label.Key, _label.Value.Item1.Invoke());
            }
        }

        private static string format(string name, string value)
            => $"<color=#888888>{name}:</color> {value}";

        public void Destroy()
        {

            Object.Destroy(root);
        }
    }
}