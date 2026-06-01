using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.GlobalState;
using PixelRouge.CsharpExtensionMethods;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    using UnityEngine.SceneManagement;
    using UnityEngine;

    public class CompatabilityChecker : IInitializable
    {
        [Inject] private CompatabilitySys sys;
        [Inject] private string systemCompatible;

        public void Initialize()
        {
            if (systemCompatible.IsNullOrEmpty())
                return;
        }
    }

    public class CompatabilitySys : IDrawable, IResetable
    {
        protected string CompatabilityReason = "Reasons";

        protected virtual string Title => "YOUR SYSTEM IS INCOMPATIBLE";
        protected virtual string SubText => "Due to:";
        protected virtual string TimerText => "The game will shutdown in:";
        protected virtual FiniteTimer EndTimer => _endTimer;

        private readonly FiniteTimer _endTimer = new FiniteTimer(10);
        [Inject] private IGameStateHandler handler;

        // ── IDrawable ─────────────────────────────────────────────────────────────
        public void Draw()
        {
            Debug.LogError("System is incompatible.");
            handler.TransitionToCriticalFailureState();
            Scene errorScene = SceneManager.CreateScene("CompatibilityError");
            SceneManager.SetActiveScene(errorScene);

            var go = new GameObject("[CompatibilityUI]");
            SceneManager.MoveGameObjectToScene(go, errorScene);
            var ui = go.AddComponent<CompatibilityUIController>();
            ui.Init(Title, SubText, CompatabilityReason, TimerText, _endTimer);
        }

        // ── IResetable ────────────────────────────────────────────────────────────
        public void ResetToDefault()
        {
            CompatabilityReason = "Reasons";
            _endTimer.reset();
        }

        // ── Convenience ───────────────────────────────────────────────────────────
        public void TriggerIncompatibility(string reason)
        {
            CompatabilityReason = reason;
            Draw();
        }
    }



    public class CompatibilityUIController : MonoBehaviour
    {
        [Header("Flash settings")] public float FlashFrequency = 2f;

        private TMP_Text _titleText;
        private TMP_Text _subText;
        private TMP_Text _reasonText;
        private TMP_Text _timerLabel;
        private TMP_Text _timerValue;
        private FiniteTimer _timer;

        private float _flashAccum;
        private bool _visible = true;

        private static readonly Color k_Red = new Color(1f, 0.08f, 0.08f);
        private static readonly Color k_DimRed = new Color(0.55f, 0.03f, 0.03f);

        public void Init(string title, string sub, string reason, string timerLabel, FiniteTimer timer)
        {
            _timer = timer;
            _timer.reset();

            BuildCanvas();

            _titleText.text = title;
            _subText.text = sub;
            _reasonText.text = reason;
            _timerLabel.text = timerLabel;
        }

        void Update()
        {
            _timer.updateTimer(Time.deltaTime);

            _timerValue.text = Mathf.CeilToInt(Mathf.Max(0f, _timer.remaingValue())).ToString();

            float half = 1f / (FlashFrequency * 2f);
            _flashAccum += Time.deltaTime;
            if (_flashAccum >= half)
            {
                _flashAccum -= half;
                _visible = !_visible;
                Color c = _visible ? k_Red : k_DimRed;
                _titleText.color = c;
                _subText.color = c;
                _reasonText.color = c;
                _timerLabel.color = c;
                _timerValue.color = c;
            }

            if (_timer.isComplete) Shutdown();
        }

        static void Shutdown()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        void BuildCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(transform);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            MakeStretched(canvasGO, "BG").AddComponent<Image>().color = Color.black;

            // ── Root vertical split: title takes all flex, footer is fixed ────────
            var root = MakeStretched(canvasGO, "Root");
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 0f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(60, 60, 40, 40);

            // ── Title block — eats all available vertical space ───────────────────
            var titleBlock = new GameObject("TitleBlock");
            titleBlock.transform.SetParent(root.transform, false);
            var titleLE = titleBlock.AddComponent<LayoutElement>();
            titleLE.flexibleHeight = 1f; // stretches to fill whatever is left
            titleLE.minHeight = 100f;

            // Title text anchored to center of its block
            _titleText = titleBlock.AddComponent<TextMeshProUGUI>();
            _titleText.fontSize = 120;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.color = k_Red;
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.enableWordWrapping = true;
            _titleText.enableAutoSizing = true; // shrinks to fit if title is long
            _titleText.fontSizeMin = 40;

            // ── Footer — fixed height, small text ────────────────────────────────
            var footer = new GameObject("Footer");
            footer.transform.SetParent(root.transform, false);
            var footerLE = footer.AddComponent<LayoutElement>();
            footerLE.minHeight = 160f;
            footerLE.flexibleHeight = 0f;

            var fvlg = footer.AddComponent<VerticalLayoutGroup>();
            fvlg.childAlignment = TextAnchor.LowerCenter;
            fvlg.spacing = 6f;
            fvlg.childForceExpandWidth = true;
            fvlg.childForceExpandHeight = false;
            fvlg.padding = new RectOffset(0, 0, 0, 0);

            // Sub + Reason on one line
            var row = new GameObject("SubReasonRow");
            row.transform.SetParent(footer.transform, false);
            var rowLE = row.AddComponent<LayoutElement>();
            rowLE.minHeight = 28f;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8f;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            _subText = MakeText(row, "Sub", 30, FontStyles.Normal);
            _reasonText = MakeText(row, "Reason", 42, FontStyles.Italic);

            // Timer label — small note
            _timerLabel = MakeText(footer, "TimerLabel", 30, FontStyles.Normal);

            // Timer value — matches title energy
            _timerValue = MakeText(footer, "TimerValue", 90, FontStyles.Bold);
            var tvLE = _timerValue.GetComponent<LayoutElement>();
            tvLE.minHeight = 90f * 1.2f;
        }

        static TMP_Text MakeText(GameObject parent, string name, float size, FontStyles style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = size * 1.4f;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.fontStyle = style;
            t.color = new Color(1f, 0.08f, 0.08f);
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            return t;
        }

        static GameObject MakeStretched(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }
    }
}