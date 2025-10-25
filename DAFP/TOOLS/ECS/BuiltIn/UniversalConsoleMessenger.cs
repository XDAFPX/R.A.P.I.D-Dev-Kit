using System;
using System.Collections.Generic;
using Codice.Utils;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Colors;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using PixelRouge.Colors;
using TMPro;
using TripleA.Utils.Extensions;
using UGizmo;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalConsoleMessenger : IMessenger, ISwitchable, IPet<IDebugSys<IGlobalGizmos, IMessenger>>
    {
        [Inject]
        public UniversalConsoleMessenger([Inject(Id = "ConsoleUnlocked")] bool unlocked,
            [Inject(Id = "ConsoleFont")] Font consoleFont, [Inject(Id = "ConsoleTextSize")] float fontSize,
            [Inject(Id = "ConsoleCommandInterpriter")]
            ICommandInterpriter interpriter)
        {
            ConsoleUnlocked = unlocked;
            this.ConsoleFont = consoleFont;
            this.fontSize = fontSize;
            TMPConsoleFont = TMP_FontAsset.CreateFontAsset(ConsoleFont);
            Interpriter = interpriter;
            Interpriter.ChangeOwner(this);
        }

        protected ICommandInterpriter Interpriter;
        public bool ConsoleUnlocked;
        protected readonly Font ConsoleFont;
        private readonly float fontSize;
        protected readonly TMP_FontAsset TMPConsoleFont;
        private bool initialized;


        protected Canvas Root;
        protected VerticalLayoutGroup CommandLineContainer;
        protected Transform SampleInputLineContainer;
        protected TMP_InputField CurrentInput;
        protected Transform SampleSetInputLineContainer;
        protected Transform SampleOutputLineContainer;
        protected readonly List<string> Inputs = new List<string>();
        protected int CurrentInputIndex;
        private List<IDebugSys<IGlobalGizmos, IMessenger>> owners = new List<IDebugSys<IGlobalGizmos, IMessenger>>();

        public void Print(IMessage message)
        {
            string val = message.Print();
            List<string> results = new List<string>();
            SplitMessageIntoLines(val, results);

            foreach (var _result in results)
            {
                SpawnOutputCommand(SampleOutputLineContainer, CommandLineContainer.GetComponent<RectTransform>(),
                    _result
                );
            }

            MoveInputToLast(SampleInputLineContainer);

            CheckIfCommandsDontFit(CommandLineContainer.GetComponent<RectTransform>(),
                Root.GetComponent<RectTransform>());
        }

        public string Procces(string input)
        {
            var result = Interpriter.Procces(input);
            if (result == null)
                return @$" '{input}' is not recognized as an internal or external command";
            return result;
        }

        public void Tick()
        {
            if (!initialized)
            {
                var data = Setup();
                Root = data.Item2;
                CommandLineContainer = data.Item1;
                SampleSetInputLineContainer = data.Item3;
                SampleOutputLineContainer = data.Item4;
                SampleInputLineContainer = data.Item5;

                SampleInputLineContainer.gameObject.SetActive(true);
                SampleOutputLineContainer.gameObject.SetActive(false);
                SampleSetInputLineContainer.gameObject.SetActive(false);

                CurrentInput = SampleInputLineContainer.GetComponentInChildren<TMP_InputField>();
                EnsureEventSystemExists(Root);
                UpdateEnableability();
                EnsureVisibility();
            }

            UpdateTerminal();
        }

        protected void UpdateTerminal()
        {
            bool ctrl = UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);
            bool shift = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
            if (ctrl && shift && UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                ConsoleUnlocked = !ConsoleUnlocked;
                if (Enabled && !ConsoleUnlocked)
                    Disable();
            }

            if (!ConsoleUnlocked)
                return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote))
            {
                Enabled = !Enabled;
                UpdateEnableability();
            }

            CommandLineContainer.transform.localPosition = new Vector3(CommandLineContainer.transform.localPosition.x,
                CommandLineContainer.transform.localPosition.y, 0);
            if (!Enabled)
                return;

            if (CurrentInput.isFocused && Input.GetKeyDown(KeyCode.UpArrow))
            {
                PasteCommand(-1);
            }

            if (CurrentInput.isFocused && Input.GetKeyDown(KeyCode.DownArrow))
            {
                PasteCommand(1);
            }

            if (CurrentInput.isFocused && UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                string input = CurrentInput.text;
                RefreshInput(CurrentInput);
                SpawnSetCMDLine(SampleSetInputLineContainer, CommandLineContainer.transform, input);
                SaveInputForLater(input);
                Print(IMessage.Literal(Procces(input)));
                MoveInputToLast(SampleInputLineContainer);
                CheckIfCommandsDontFit(CommandLineContainer.GetComponent<RectTransform>(),
                    Root.GetComponent<RectTransform>());
            }

            SelectField(CurrentInput);
        }

        private void SaveInputForLater(string input)
        {
            var trimmed = Inputs.Clone();
            for (int i = 0; i < trimmed.Count; i++)
            {
                trimmed[i] = trimmed[i].Trim(" `".ToCharArray());
            }

            if (!trimmed.Contains(input.Trim(" `".ToCharArray())))
            {
                Inputs.Add(input);
                CurrentInputIndex = Inputs.Count;
            }
            else
            {
                int i = trimmed.FindIndex((s => s == input.Trim(" `".ToCharArray())));
                CurrentInputIndex = i + 1;
            }
        }

        private void PasteCommand(int i)
        {
            if (Inputs.IsInBounds(CurrentInputIndex + i))
                CurrentInputIndex += i;
            if (Inputs.IsInBounds(CurrentInputIndex))
            {
                CurrentInput.text = Inputs[CurrentInputIndex];
                var field = CurrentInput;
                field.caretPosition = Inputs[CurrentInputIndex].Length;
                field.selectionAnchorPosition = Inputs[CurrentInputIndex].Length;
                field.selectionFocusPosition = Inputs[CurrentInputIndex].Length;
                field.ForceLabelUpdate();
            }
        }

        private void EnsureVisibility(bool again = false)
        {
            if (Root.renderMode == RenderMode.ScreenSpaceCamera && Root.worldCamera == null)
            {
                if (again)
                {
                    Root.renderMode = RenderMode.ScreenSpaceOverlay;
                    return;
                }

                Root.worldCamera = Camera.main;
                EnsureVisibility(true);
            }
        }

        private void UpdateEnableability()
        {
            if (Enabled)
                Enable();
            else
            {
                Disable();
            }
        }

        protected void CheckIfCommandsDontFit(RectTransform Container, RectTransform root)
        {
            var myContentFitter = Container;
            LayoutRebuilder.MarkLayoutForRebuild(myContentFitter);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(myContentFitter);


            if (Container.sizeDelta.y > root.sizeDelta.y)
            {
                var move = Mathf.Abs(Mathf.Abs(Container.sizeDelta.y - root.sizeDelta.y) -
                                     Container.anchoredPosition.y);
                Container.anchoredPosition += new Vector2(0, move);
            }
        }

        protected void MoveInputToLast(Transform currentInput)
        {
            currentInput.transform.SetAsLastSibling();
        }

        protected void SpawnSetCMDLine(Transform setInput, Transform container, string txt)
        {
            var dupl = GameObject.Instantiate(setInput, container);

            dupl.GetComponentsInChildren<TextMeshProUGUI>()[1].text = txt;
            dupl.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(dupl.GetChild(0).GetComponent<RectTransform>());
        }

        protected void SpawnOutputCommand(Transform output, Transform container, string txt)
        {
            var dupl = GameObject.Instantiate(output, container);

            dupl.GetComponentsInChildren<TextMeshProUGUI>()[0].text = txt;
            dupl.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(dupl.GetChild(0).GetComponent<RectTransform>());
        }

        protected void RefreshInput(TMP_InputField input)
        {
            input.text = "";
            SelectField(input);
        }

        private static void SelectField(TMP_InputField input)
        {
            input.ActivateInputField();

            input.Select();
        }

        protected virtual ColorScheme GetColorScheme() => new ComplementaryScheme(ColorsForUnity.Honeydew);

        protected virtual string GetDefaultPrefix() =>
            @"C:\Users\Admin > ";

        protected (VerticalLayoutGroup, Canvas, Transform, Transform, Transform, ColorScheme)
            Setup()
        {
            initialized = true;
            var canvas = RootInit(out var root, out var cmdcontainer, out var vertical);
            ConfigureLayout(vertical, true);
            vertical.GetComponent<RectTransform>().AnchorTopLeft();
            vertical.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            ConfigureCMDLineContainer(cmdcontainer, out var sample_command_input, out var sample_input_rect);
            ConfigureCMDLineContainer(cmdcontainer, out var sampleCommand, out var sample_rect);
            ConfigureCMDLineContainer(cmdcontainer, out var sampleComman, out var sample_output_rect);
            var sample_text = ConfigureSetInputField(sample_rect, GetDefaultPrefix());
            var sample_output = ConfigureOutput(sample_output_rect, GetDefaultPrefix());

            var sample_input = ConfigureInputField(sample_input_rect, GetDefaultPrefix());


            return (vertical, canvas, sample_rect, sample_output_rect, sample_input_rect, GetColorScheme());
        }


        private TMP_InputField ConfigureInputField(RectTransform sample_rect, string prefix)
        {
            var sample_text = ConfigureTextLine(sample_rect, prefix, out var sample_dir, out var text_container);
            var sample_input = new GameObject("input").AddComponent<TMP_InputField>();
            sample_input.gameObject.AddOrGetComponent<RectTransform>();
            sample_input.transform.SetParent(sample_text.transform.parent);
            GameObject.Destroy(sample_text.gameObject);
            ConfigureInputFieldForText(sample_input, TMPConsoleFont, text_container, sample_dir, "", "", fontSize);
            text_container.transform.localScale = Vector3.one;
            sample_dir.transform.localScale = Vector3.one;
            sample_input.transform.localScale = Vector3.one;
            LayoutRebuilder.ForceRebuildLayoutImmediate(text_container.GetComponent<RectTransform>());
            return sample_input;
        }

        private TextMeshProUGUI ConfigureOutput(RectTransform sample_rect, string prefix)
        {
            var sample_text = ConfigureTextLine(sample_rect, prefix, out var sample_dir, out var text_container);
            GameObject.Destroy(sample_dir.gameObject);

            ConfigureText(sample_text, TMPConsoleFont, "sample output", fontSize);
            text_container.transform.localScale = Vector3.one;
            sample_text.transform.localScale = Vector3.one;
            LayoutRebuilder.ForceRebuildLayoutImmediate(text_container.GetComponent<RectTransform>());
            return sample_text;
        }

        private TextMeshProUGUI ConfigureSetInputField(RectTransform sample_rect, string prefix)
        {
            var sample_text = ConfigureTextLine(sample_rect, prefix, out var sample_dir, out var text_container);
            ConfigureText(sample_text, TMPConsoleFont, "sample input", fontSize);
            text_container.transform.localScale = Vector3.one;
            sample_text.transform.localScale = Vector3.one;
            sample_dir.transform.localScale = Vector3.one;
            LayoutRebuilder.ForceRebuildLayoutImmediate(text_container.GetComponent<RectTransform>());
            return sample_text;
        }

        private TextMeshProUGUI ConfigureTextLine(RectTransform sample_rect, string prefix,
            out TextMeshProUGUI sample_dir,
            out HorizontalLayoutGroup text_container)
        {
            var sample_text = new GameObject("txt").AddComponent<TextMeshProUGUI>();

            sample_dir = new GameObject("dir").AddComponent<TextMeshProUGUI>();
            text_container = new GameObject("txt_container").AddComponent<HorizontalLayoutGroup>();
            text_container.transform.SetParent(sample_rect);
            text_container.GetComponent<RectTransform>().AnchorTopLeft();
            ConfigureLayout(text_container);
            var a = text_container.padding;
            a.left += 5;
            text_container.padding = a;

            sample_dir.transform.SetParent(text_container.transform);
            ConfigureText(sample_dir, TMPConsoleFont, prefix, fontSize);
            sample_text.transform.SetParent(text_container.transform);
            return sample_text;
        }

        private static void ConfigureCMDLineContainer(GameObject cmdcontainer, out GameObject sample_command_input,
            out RectTransform sample_rect)
        {
            sample_command_input = new GameObject("CMD 101");
            // sample_command_input.SetActive(false);
            sample_command_input.transform.SetParent(cmdcontainer.transform);
            sample_rect = sample_command_input.AddOrGetComponent<RectTransform>();
            sample_rect.sizeDelta = new Vector2(0, 40);
            sample_rect.localScale = Vector3.one;
        }

        private static void ConfigureText(TextMeshProUGUI text, TMP_FontAsset font, string txt, float size,
            bool dont_resize = false)
        {
            if (text == null)
            {
                Debug.LogWarning("UITextUtils.ConfigureText() called with null text reference.");
                return;
            }

            // Base properties
            text.color = Color.white;
            text.font = font;
            text.overflowMode = TextOverflowModes.Overflow;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.alignment = TextAlignmentOptions.Left;
            text.text = txt;
            text.fontSize = size;
            if (!dont_resize)
                ResizeToFitText(text);

            // RectTransform configuration
            var rect = text.gameObject.AddOrGetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0, 40);
        }

        /// <summary>
        /// Configures a TMP_InputField with standard styling, font, text, and size.
        /// </summary>
        /// <param name="inputField">The TMP_InputField to configure.</param>
        /// <param name="font">Font to use for the input text.</param>
        /// <param name="placeholderText">Optional placeholder text.</param>
        /// <param name="initialText">Optional initial text.</param>
        /// <param name="fontSize">Font size.</param>
        public static void ConfigureInputFieldForText(
            TMP_InputField inputField,
            TMP_FontAsset font, HorizontalOrVerticalLayoutGroup group,
            TextMeshProUGUI dir,
            string placeholderText = "",
            string initialText = "",
            float fontSize = 36f)
        {
            if (inputField == null)
            {
                Debug.LogWarning(" called with null inputField.");
                return;
            }


            inputField.transform.GetComponent<RectTransform>().sizeDelta =
                new Vector2(2000, dir.GetPreferredValues().y);
            var img = inputField.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.sprite = Sprite.Create(new Texture2D(2, 2), new Rect(Vector2.one, Vector2.one), Vector2.zero);
            inputField.caretWidth = 10;
            inputField.customCaretColor = true;
            inputField.caretColor = Color.white;
            inputField.caretBlinkRate = (float)(Math.PI * 0.3);
            inputField.onValueChanged.AddListener((arg0 =>
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>())));
            inputField.transition = Selectable.Transition.None;

            // var cc = inputField.gameObject.AddComponent<ContentSizeFitter>();
            // cc.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            // cc.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


            // Setup Text Component
            var text_rect = new GameObject("rect");
            text_rect.transform.SetParent(inputField.transform);
            text_rect.AddOrGetComponent<RectTransform>().StretchFull();
            inputField.textViewport = text_rect.GetComponent<RectTransform>();
            var txt = new GameObject("inputText").AddComponent<TextMeshProUGUI>();
            txt.transform.SetParent(text_rect.transform);
            inputField.textComponent = txt;
            var textComponent = inputField.textComponent;
            ConfigureText(textComponent.GetComponent<TextMeshProUGUI>(), font, initialText, fontSize, true);
            txt.gameObject.AddOrGetComponent<RectTransform>().AnchorMiddleLeft();
            txt.gameObject.AddOrGetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            inputField.selectionColor = Color.gray;

            inputField.enabled = false;
            inputField.enabled = true;
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
            inputField.ForceLabelUpdate();

            // Setup Placeholder
            // if (inputField.placeholder is TextMeshProUGUI placeholder)
            // {
            //     placeholder.font = font;
            //     placeholder.fontSize = fontSize;
            //     placeholder.color = new Color(1f, 1f, 1f, 0.5f); // semi-transparent
            //     placeholder.alignment = TextAlignmentOptions.Left;
            //     placeholder.text = placeholderText;
            // }

            // Optionally, set RectTransform for inputField itself
            // var inputRect = inputField.GetComponent<RectTransform>();
            // if (inputRect != null)
            // {
            //     inputRect.sizeDelta = new Vector2(0, 40);
            // }
        }

        public static void ResizeToFitText(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;

            // Force TMP to update its internal layout data first
            tmp.ForceMeshUpdate();
            var fit = tmp.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // Get preferred size of the text
        }

        private Canvas RootInit(out GameObject root, out GameObject cmdcontainer, out VerticalLayoutGroup vertical)
        {
            root = new GameObject("ConsoleRoot");

            var scroll_root = new GameObject("root_");
            var scroll = scroll_root.AddComponent<ScrollRect>();
            GameObject.DontDestroyOnLoad(root);
            var canvas = root.AddComponent<Canvas>();
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;


            scroll_root.transform.SetParent(root.transform, false);
            scroll_root.transform.localScale = Vector3.one;
            scroll_root.GetComponent<RectTransform>().AnchorTopLeft();
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            scroll.scrollSensitivity = 50;


            cmdcontainer = new GameObject(nameof(CommandLineContainer));
            cmdcontainer.transform.SetParent(scroll_root.transform);
            var cc = cmdcontainer.AddComponent<ContentSizeFitter>();
            cc.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            cc.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            vertical = cmdcontainer.AddComponent<VerticalLayoutGroup>();
            scroll.content = cmdcontainer.transform.GetComponent<RectTransform>();

            scroll.horizontal = false;
            var handle = new GameObject("console handle").transform;
            handle.SetParent(scroll_root.transform);
            var handle_rect = handle.AddOrGetComponent<RectTransform>();
            handle_rect.AnchorMiddleLeft();
            handle_rect.sizeDelta = new Vector2(5, 2000);
            handle_rect.anchoredPosition = Vector2.zero;
            handle_rect.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);


            return canvas;
        }

        private static void ConfigureLayout(HorizontalOrVerticalLayoutGroup layout, bool stretch = false)
        {
            if (layout == null)
            {
                Debug.LogWarning("UILayoutUtils.ConfigureLayout() called with null layout.");
                return;
            }

            layout.childControlHeight = false;
            layout.childControlWidth = false;
            // Disable child expansion
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childScaleWidth = true;
            layout.childScaleHeight = true;
            // Optional alignment (commented out if you want to keep user defaults)
            // layout.childAlignment = TextAnchor.UpperCenter;

            // Apply padding
            var padding = layout.padding;
            padding.top = 5;
            layout.padding = padding;
            var rect = layout.GetComponent<RectTransform>();
            // Apply spacing
            layout.spacing = 5;
            if (stretch)
            {
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            rect.localScale = Vector3.one;
        }


        public static void EnsureEventSystemExists(Canvas parentCanvas = null)
        {
            // already present?
            if (EventSystem.current != null)
                return;

            var go = new GameObject("EventSystem", typeof(EventSystem));

            // Try to attach the correct input module:
            if (TryAddInputSystemUIModule(go) == false)
            {
                // fallback to old input system
                go.AddComponent<StandaloneInputModule>();
            }

            if (parentCanvas != null)
                go.transform.SetParent(parentCanvas.transform, false);

            Object.DontDestroyOnLoad(go); // optional, keeps it persistent

            Debug.Log("[UIUtils] Created EventSystem with proper input module!");
        }

#if ENABLE_INPUT_SYSTEM
        private static bool TryAddInputSystemUIModule(GameObject go)
        {
            // new Input System present
            var type = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (type != null)
            {
                go.AddComponent(type);
                return true;
            }

            return false;
        }
#else
        private static bool TryAddInputSystemUIModule(GameObject go) => false;
#endif
        public static void SplitMessageIntoLines(string message, List<string> output)
        {
            if (message == null || output == null)
                throw new ArgumentNullException();

            var lines = message.Split(new[] { "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    output.Add(trimmed);
                }
            }
        }

        public bool Enabled { set; get; }

        public void Enable()
        {
            Enabled = true;
            Root.gameObject.SetActive(true);
        }

        public void Disable()
        {
            Enabled = false;
            Root.gameObject.SetActive(false);
        }

        public ISet<IOwnable<ICommandInterpriter>> Pets => new HashSet<IOwnable<ICommandInterpriter>>() { Interpriter };
        public List<ICommandInterpriter> Owners { get; } = new List<ICommandInterpriter>();

        List<IDebugSys<IGlobalGizmos, IMessenger>> IPet<IDebugSys<IGlobalGizmos, IMessenger>>.Owners => owners;
    }
}