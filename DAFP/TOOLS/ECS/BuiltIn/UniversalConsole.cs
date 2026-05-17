using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using R3;
using RapidLib.DAFP.TOOLS.Common;
using TMPro;
using TripleA.Utils.Extensions;
using UGizmo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalConsole : IConsoleMessenger, ISwitchable, IDebugSubSys,
        IPetOf<IDebugSys<IGlobalGizmos, IConsoleMessenger>, IDebugSubSys>
    {
        [Inject]
        public UniversalConsole([Inject(Id = "ConsoleUnlocked")] bool unlocked,
            [Inject(Id = "ConsoleCommandInterpreter")]
            ICommandInterpreter interpreter, ControllerManager controllerManager)
        {
            ConsoleUnlocked = unlocked;
            Interpreter = interpreter;
            this.controllerManager = controllerManager;
            Interpreter.ChangeOwner(this);

            this.ConsoleColor = Color.white;

            this.ConsoleFontSize = 23;
            ConsoleFont = Font.CreateDynamicFontFromOSFont(Font.GetOSInstalledFontNames()[0], 32);
            TMPConsoleFont = TMP_FontAsset.CreateFontAsset(ConsoleFont);
        }

        protected ICommandInterpreter Interpreter;
        private readonly ControllerManager controllerManager;
        protected readonly Color ConsoleColor;
        public bool ConsoleUnlocked;
        protected readonly Font ConsoleFont;
        protected float ConsoleFontSize;
        protected TMP_FontAsset TMPConsoleFont;
        private bool initialized;


        protected Canvas Root;
        protected VerticalLayoutGroup CommandLineContainer;
        protected Transform CurrentInputLineContainer;
        protected TMP_InputField CurrentInput;
        protected Transform SampleSetInputLineContainer;
        protected Transform SampleOutputLineContainer;
        protected readonly List<string> Inputs = new();
        protected int CurrentInputIndex;

        // protected IEnumerable<Transform> AllPrintedLines =>
        //     collect_all_printed_lines(CommandLineContainer.transform); //all lines including set ones not including

        protected IEnumerable<Transform> AllProcessPrintedLines =>
            collect_all_process_printed_lines(CommandLineContainer
                .transform).Values; //all lines including set ones not including

        //lines like the console's current input nor the first 2 disabled ones
        protected int CursorY = 0;

        public virtual void Print(IMessage message)
        {
            if (!initialized)
                return;
            var _val = ExtractStringFromMessage(message);
            var _results = new List<string>();
            SplitMessageIntoLines(_val, _results);

            MoveInputToLast(CurrentInputLineContainer);

            handle_each_output(_results);

            MoveInputToLast(CurrentInputLineContainer);

            CheckIfCommandsDontFit(CommandLineContainer.GetComponent<RectTransform>(),
                Root.GetComponent<RectTransform>());
        }

        private void handle_each_output(List<string> _results)
        {
            foreach (var _result in _results)
            {
                var _finalResult = handle_cursor_logic(_result);
                if (CursorY > collect_all_process_printed_lines(CommandLineContainer.transform).Count)
                {
                    SpawnOutputCommandOrOverride(SampleOutputLineContainer, CommandLineContainer.transform,
                        _finalResult
                    );
                }
                else
                {
                    SpawnOutputCommandOrOverride(SampleOutputLineContainer, CommandLineContainer.transform,
                        _finalResult, CursorY
                    );
                }
            }
        }

        private string handle_cursor_logic(string _val)
        {
            CursorToBottom();
            _val = ParseANSI(_val);
            return _val;
        }

        protected virtual string ParseANSI(string raw) //--cursor control
        {
            var up = Regex.Match(raw, @"\x1b\[(\d+)A");
            if (up.Success) CursorUp(int.Parse(up.Groups[1].Value));

            var down = Regex.Match(raw, @"\x1b\[(\d+)B");
            if (down.Success) CursorDown(int.Parse(down.Groups[1].Value));

            // strip remaining escape codes and print clean text
            var clean = Regex.Replace(raw, @"\x1b\[[^m]*m|\x1b\[\d+[AB]", "");
            return clean;
        }

        public void MoveCursor(int delta)
        {
            CursorY = Mathf.Clamp(CursorY + delta, 0, AllProcessPrintedLines.Count() + 1);
        }

        public void CursorUp(int n = 1) => MoveCursor(-n);
        public void CursorDown(int n = 1) => MoveCursor(n);
        public void CursorToBottom() => MoveCursor(10000);

        public static bool ContainsANSI(string input)
        {
            return Regex.IsMatch(input, @"\x1b\[[^a-zA-Z]*[a-zA-Z]");
        }

// strips all ANSI escape codes from string
        public static string StripANSI(string input)
        {
            return Regex.Replace(input, @"\x1b\[[^a-zA-Z]*[a-zA-Z]", "");
        }

        protected virtual string ExtractStringFromMessage(IMessage message)
        {
            var _val = message.Print();
            return _val;
        }

        public void Clear()
        {
            clear(CommandLineContainer.transform);
        }

        private Dictionary<int, Transform> collect_all_process_printed_lines(Transform container)
        {
            var _result = new Dictionary<int, Transform>();
            for (int _i = 0; _i < container.childCount; _i++)
            {
                var _child = container.GetChild(_i);
                if (_child.gameObject.name == "SetInput line")
                    continue;
                bool _isLast = _child.gameObject == CurrentInput.gameObject ||
                               _child.gameObject == CurrentInputLineContainer.gameObject;
                if (!_isLast && _child.gameObject.activeSelf)
                    _result.Add(_i, _child);
            }

            return _result;
        }

        private IEnumerable<Transform> collect_all_printed_lines(Transform container)
        {
            for (int _i = container.childCount - 1; _i >= 0; _i--)
            {
                var _child = container.GetChild(_i);
                bool _isLast = _child.gameObject == CurrentInput.gameObject ||
                               _child.gameObject == CurrentInputLineContainer.gameObject;
                if (!_isLast && _child.gameObject.activeSelf)
                    yield return _child;
            }
        }

        private void clear(Transform container)
        {
            collect_all_printed_lines(container).ForEach((transform => GameObject.Destroy(transform.gameObject)));
        }

        public virtual ITextProcess Process(string input)
        {
            var _result = Interpreter.Process(input);
            if (_result == null)
                return ITextProcess.Literal($" '{input}' is not recognized as an internal or external command");
            return _result;
        }

        public void Tick()
        {
            if (!initialized)
            {
                Init();
            }

            UpdateTerminal();
        }

        protected virtual void Init()
        {
            var _data = Setup();
            Root = _data.Item2;
            CommandLineContainer = _data.Item1;
            SampleSetInputLineContainer = _data.Item3;
            SampleOutputLineContainer = _data.Item4;
            CurrentInputLineContainer = _data.Item5;

            CurrentInputLineContainer.gameObject.SetActive(true);
            SampleOutputLineContainer.gameObject.SetActive(false);
            SampleSetInputLineContainer.gameObject.SetActive(false);

            CurrentInput = CurrentInputLineContainer.GetComponentInChildren<TMP_InputField>();
            EnsureEventSystemExists(Root);
            ensure_visibility();
            Disable();
            OutputStream = new Subject<IMessage>();
            OutputStream.Subscribe(Print);
        }

        protected virtual void UpdateTerminal()
        {
            var _ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var _shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (_ctrl && _shift && Input.GetKeyDown(KeyCode.D))
            {
                ConsoleUnlocked = !ConsoleUnlocked;
                if (Enabled && !ConsoleUnlocked)
                    Disable();
            }

            if (!ConsoleUnlocked)
                return;
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                Enabled = !Enabled;
            }

            CommandLineContainer.transform.localPosition = new Vector3(CommandLineContainer.transform.localPosition.x,
                CommandLineContainer.transform.localPosition.y, 0);
            if (!Enabled)
                return;
            if (CurrentProcess != null)
            {
                if (_ctrl && Input.GetKeyDown(KeyCode.C))
                {
                    cts.Cancel();
                    dispose_of_current_process();
                    Debug.Log("[Console] Process canceled");
                }

                return;
            }


            UpdateTerminalEnabled(_ctrl);
        }

        protected ITextProcess CurrentProcess;
        private CancellationTokenSource cts;
        public Subject<IMessage> OutputStream;


        protected virtual void UpdateTerminalEnabled(bool ctrl)
        {
            if (CurrentInput.isFocused && Input.GetKeyDown(KeyCode.UpArrow)) PasteCommand(-1);

            if (CurrentInput.isFocused && Input.GetKeyDown(KeyCode.DownArrow)) PasteCommand(1);

            if (CurrentInput.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                HandleInput().Forget();
            }

            if (CurrentInput.isFocused && ctrl && Input.GetKeyDown(KeyCode.L))
            {
                clear(CommandLineContainer.transform);
            }

            select_field(CurrentInput);
        }

        protected virtual async UniTask HandleInput()
        {
            cts = new CancellationTokenSource();
            var _input = CurrentInput.text;
            var _result = Process(_input);
            CurrentProcess = _result;
            var _ctx = new TextProcessContext() { Log = OutputStream };


            save_input_for_later(_input);
            RefreshInput(CurrentInput);
            SpawnSetCmdLine(SampleSetInputLineContainer, CommandLineContainer.transform, _input);

            MoveInputToLast(CurrentInputLineContainer);

            CurrentInputLineContainer.gameObject.SetActive(false);

            try
            {
                await CurrentProcess.Execute(_ctx, cts.Token);
                Print(IMessage.Literal("  "));
            }
            catch (OperationCanceledException)
            {
                Print(IMessage.Literal("  ^C"));
            }


            CurrentInputLineContainer.gameObject.SetActive(true);
            dispose_of_current_process();
            //--meta
            RefreshInput(CurrentInput);
            MoveInputToLast(CurrentInputLineContainer);
            CheckIfCommandsDontFit(CommandLineContainer.GetComponent<RectTransform>(),
                Root.GetComponent<RectTransform>());
        }


        private void dispose_of_current_process()
        {
            CurrentProcess = null;
            cts.Dispose();
        }

        private void save_input_for_later(string input)
        {
            var _trimmed = Inputs.Clone();
            for (var _i = 0; _i < _trimmed.Count; _i++) _trimmed[_i] = _trimmed[_i].Trim(" `".ToCharArray());

            if (!_trimmed.Contains(input.Trim(" `".ToCharArray())))
            {
                Inputs.Add(input);
                CurrentInputIndex = Inputs.Count;
            }
            else
            {
                var _i = _trimmed.FindIndex(s => s == input.Trim(" `".ToCharArray()));
                CurrentInputIndex = _i + 1;
            }
        }

        protected virtual void PasteCommand(int i)
        {
            if (Inputs.IsInBounds(CurrentInputIndex + i))
                CurrentInputIndex += i;
            if (Inputs.IsInBounds(CurrentInputIndex))
            {
                CurrentInput.text = Inputs[CurrentInputIndex];
                var _field = CurrentInput;
                _field.caretPosition = Inputs[CurrentInputIndex].Length;
                _field.selectionAnchorPosition = Inputs[CurrentInputIndex].Length;
                _field.selectionFocusPosition = Inputs[CurrentInputIndex].Length;
                _field.ForceLabelUpdate();
            }
        }

        protected virtual void ensure_visibility(bool again = false)
        {
            if (Root.renderMode == RenderMode.ScreenSpaceCamera && Root.worldCamera == null)
            {
                if (again)
                {
                    Root.renderMode = RenderMode.ScreenSpaceOverlay;
                    return;
                }

                Root.worldCamera = Camera.main;
                ensure_visibility(true);
            }
        }


        protected void CheckIfCommandsDontFit(RectTransform container, RectTransform root)
        {
            var _myContentFitter = container;
            LayoutRebuilder.MarkLayoutForRebuild(_myContentFitter);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_myContentFitter);


            if (container.sizeDelta.y > root.sizeDelta.y)
            {
                var _move = Mathf.Abs(Mathf.Abs(container.sizeDelta.y - root.sizeDelta.y) -
                                      container.anchoredPosition.y);
                container.anchoredPosition += new Vector2(0, _move);
            }
        }

        protected void MoveInputToLast(Transform currentInput)
        {
            currentInput.transform.SetAsLastSibling();
        }

        protected void SpawnSetCmdLine(Transform setInput, Transform container, string txt)
        {
            var _dupl = GameObject.Instantiate(setInput, container);
            _dupl.gameObject.name = "SetInput line";
            _dupl.GetComponentsInChildren<TextMeshProUGUI>()[1].text = txt;
            _dupl.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_dupl.GetChild(0).GetComponent<RectTransform>());
        }

        protected virtual bool IsTheOutputSpace(string output)
        {
            return output.IsNullOrWhiteSpace();
        }

        protected void SpawnOutputCommandOrOverride(Transform output, Transform container,
            string txt, int? childIndex = null)
        {
            try //--log any errors cuz they will be ignored if not 
            {
                if (IsTheOutputSpace(txt))
                    txt = "";
                var _lines = collect_all_process_printed_lines(CommandLineContainer.transform);

                int _convertedChildIndex = 0;
                if (childIndex.HasValue)
                {
                    _convertedChildIndex = _lines.Keys.ElementAt(childIndex.Value - 1);
                    GameObject.Destroy(_lines[_convertedChildIndex].gameObject);
                }

                var _dupl = GameObject.Instantiate(output, container);
                _dupl.GetComponentsInChildren<TextMeshProUGUI>()[0].text = txt;
                _dupl.gameObject.SetActive(true);
                _dupl.gameObject.name = "Command line " + Guid.NewGuid().ToString();

                if (childIndex.HasValue)
                    _dupl.SetSiblingIndex(_convertedChildIndex);

                LayoutRebuilder.ForceRebuildLayoutImmediate(_dupl.GetChild(0).GetComponent<RectTransform>());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        protected void RefreshInput(TMP_InputField input)
        {
            input.text = "";
            select_field(input);
        }

        private static void select_field(TMP_InputField input)
        {
            input.ActivateInputField();

            input.Select();
        }


        protected virtual string GetDefaultInputFieldPrefix()
        {
            return @"C:\Users\Admin > ";
        }

        protected virtual int GetDefaultCommandLineHight()
        {
            return 40;
        }


        protected (VerticalLayoutGroup, Canvas, Transform, Transform, Transform)
            Setup()
        {
            initialized = true;
            var _canvas = root_init(out var _root, out var _cmdcontainer, out var _vertical);
            configure_layout(_vertical, true);
            _vertical.GetComponent<RectTransform>().AnchorTopLeft();
            _vertical.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            configure_cmd_line_container(_cmdcontainer, out var _sampleCommandInput, out var _sampleInputRect);
            configure_cmd_line_container(_cmdcontainer, out var _sampleCommand, out var _sampleRect);
            configure_cmd_line_container(_cmdcontainer, out var _sampleComman, out var _sampleOutputRect);
            var _sampleText = configure_set_input_field(_sampleRect, GetDefaultInputFieldPrefix(), ConsoleColor);
            var _sampleOutput = configure_output(_sampleOutputRect, GetDefaultInputFieldPrefix(), ConsoleColor);

            var _sampleInput = configure_input_field(_sampleInputRect, GetDefaultInputFieldPrefix(), ConsoleColor);


            return (_vertical, _canvas, _sampleRect, _sampleOutputRect, _sampleInputRect);
        }


        private TMP_InputField configure_input_field(RectTransform sampleRect, string prefix, Color color)
        {
            var _sampleText =
                configure_text_line(sampleRect, prefix, color, out var _sampleDir, out var _textContainer);
            var _sampleInput = new GameObject("input").AddComponent<TMP_InputField>();
            _sampleInput.gameObject.AddOrGetComponent<RectTransform>();
            _sampleInput.transform.SetParent(_sampleText.transform.parent);
            GameObject.Destroy(_sampleText.gameObject);
            sampleRect.gameObject.name = "Input continer";
            ConfigureInputFieldForText(_sampleInput, TMPConsoleFont, _textContainer, _sampleDir, color, "", "",
                ConsoleFontSize);
            _textContainer.transform.localScale = Vector3.one;
            _sampleDir.transform.localScale = Vector3.one;
            _sampleInput.transform.localScale = Vector3.one;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_textContainer.GetComponent<RectTransform>());
            return _sampleInput;
        }

        private TextMeshProUGUI configure_output(RectTransform sampleRect, string prefix, Color color)
        {
            var _sampleText =
                configure_text_line(sampleRect, prefix, color, out var _sampleDir, out var _textContainer);
            GameObject.Destroy(_sampleDir.gameObject);

            configure_text(_sampleText, TMPConsoleFont, "sample output", ConsoleFontSize, color);
            _textContainer.transform.localScale = Vector3.one;
            _sampleText.transform.localScale = Vector3.one;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_textContainer.GetComponent<RectTransform>());
            return _sampleText;
        }

        private TextMeshProUGUI configure_set_input_field(RectTransform sampleRect, string prefix, Color color)
        {
            var _sampleText =
                configure_text_line(sampleRect, prefix, color, out var _sampleDir, out var _textContainer);
            configure_text(_sampleText, TMPConsoleFont, "sample input", ConsoleFontSize, color);
            _textContainer.transform.localScale = Vector3.one;
            _sampleText.transform.localScale = Vector3.one;
            _sampleDir.transform.localScale = Vector3.one;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_textContainer.GetComponent<RectTransform>());
            return _sampleText;
        }

        protected virtual TextMeshProUGUI configure_text_line(RectTransform sampleRect, string prefix, Color color,
            out TextMeshProUGUI sampleDir,
            out HorizontalLayoutGroup textContainer)
        {
            var _sampleText = new GameObject("txt").AddComponent<TextMeshProUGUI>();

            sampleDir = new GameObject("dir").AddComponent<TextMeshProUGUI>();
            textContainer = new GameObject("txt_container").AddComponent<HorizontalLayoutGroup>();
            textContainer.transform.SetParent(sampleRect);
            textContainer.GetComponent<RectTransform>().AnchorTopLeft();
            configure_layout(textContainer);
            adjust_text_container_padding(textContainer, out var _a);

            sampleDir.transform.SetParent(textContainer.transform);
            configure_text(sampleDir, TMPConsoleFont, prefix, ConsoleFontSize, color);
            _sampleText.transform.SetParent(textContainer.transform);
            return _sampleText;
        }

        protected virtual void adjust_text_container_padding(HorizontalLayoutGroup textContainer, out RectOffset _a)
        {
            _a = textContainer.padding;
            _a.left += 5;
            textContainer.padding = _a;
        }

        protected virtual void configure_cmd_line_container(GameObject cmdcontainer, out GameObject sampleCommandInput,
            out RectTransform sampleRect)
        {
            sampleCommandInput = new GameObject("CMD 101");
            // sample_command_input.SetActive(false);
            sampleCommandInput.transform.SetParent(cmdcontainer.transform);
            sampleRect = sampleCommandInput.AddOrGetComponent<RectTransform>();
            sampleRect.sizeDelta = new Vector2(0, GetDefaultCommandLineHight());
            sampleRect.localScale = Vector3.one;
        }


        private static void configure_text(TextMeshProUGUI text, TMP_FontAsset font, string txt, float size,
            Color color,
            bool dontResize = false)
        {
            if (text == null)
            {
                Debug.LogWarning("UITextUtils.ConfigureText() called with null text reference.");
                return;
            }

            // Base properties
            text.color = color;
            text.font = font;
            text.overflowMode = TextOverflowModes.Overflow;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.alignment = TextAlignmentOptions.Left;
            text.text = txt;
            text.fontSize = size;
            if (!dontResize)
                ResizeToFitText(text);

            // RectTransform configuration
            var _rect = text.gameObject.AddOrGetComponent<RectTransform>();
            _rect.anchorMin = new Vector2(0, 1);
            _rect.anchorMax = new Vector2(0, 1);
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.sizeDelta = new Vector2(0, 40);
        }

        /// <summary>
        /// Configures a TMP_InputField with standard styling, font, text, and size.
        /// </summary>
        /// <param name="inputField">The TMP_InputField to configure.</param>
        /// <param name="font">Font to use for the input text.</param>
        /// <param name="placeholderText">Optional placeholder text.</param>
        /// <param name="initialText">Optional initial text.</param>
        /// <param name="fontSize">Font size.</param>
        protected virtual void ConfigureInputFieldForText(
            TMP_InputField inputField,
            TMP_FontAsset font, HorizontalOrVerticalLayoutGroup group,
            TextMeshProUGUI dir, Color color,
            string placeholderText = "",
            string initialText = "",
            float fontSize = 36f)
        {
            if (inputField == null)
            {
                Debug.LogWarning(" Console's InputField called with null inputField.");
                return;
            }

            inputField.gameObject.name = "Console's Input";
            inputField.transform.GetComponent<RectTransform>().sizeDelta =
                new Vector2(2000, dir.GetPreferredValues().y);
            var _img = inputField.gameObject.AddComponent<Image>();
            _img.color = new Color(0, 0, 0, 0);
            _img.sprite = Sprite.Create(new Texture2D(2, 2), new Rect(Vector2.one, Vector2.one), Vector2.zero);
            ConfigureCaretAppearance(inputField, color);
            inputField.onValueChanged.AddListener(arg0 =>
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.GetComponent<RectTransform>()));
            inputField.transition = Selectable.Transition.None;

            // var cc = inputField.gameObject.AddComponent<ContentSizeFitter>();
            // cc.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            // cc.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


            // Setup Text Component
            var _textRect = new GameObject("rect");
            _textRect.transform.SetParent(inputField.transform);
            _textRect.AddOrGetComponent<RectTransform>().StretchFull();
            inputField.textViewport = _textRect.GetComponent<RectTransform>();
            var _txt = new GameObject("inputText").AddComponent<TextMeshProUGUI>();
            _txt.transform.SetParent(_textRect.transform);
            inputField.textComponent = _txt;
            var _textComponent = inputField.textComponent;
            configure_text(_textComponent.GetComponent<TextMeshProUGUI>(), font, initialText, fontSize, color, true);
            _txt.gameObject.AddOrGetComponent<RectTransform>().AnchorMiddleLeft();
            _txt.gameObject.AddOrGetComponent<RectTransform>().anchoredPosition = Vector2.zero;

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

        protected virtual void ConfigureCaretAppearance(TMP_InputField inputField, Color color)
        {
            inputField.caretWidth = 10;
            inputField.customCaretColor = true;
            inputField.caretColor = color;
            inputField.caretBlinkRate = (float)(Math.PI * 0.3);
        }

        public static void ResizeToFitText(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;

            // Force TMP to update its internal layout data first
            tmp.ForceMeshUpdate();
            var _fit = tmp.gameObject.AddComponent<ContentSizeFitter>();
            _fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // Get preferred size of the text
        }

        protected virtual Canvas root_init(out GameObject root, out GameObject cmdcontainer,
            out VerticalLayoutGroup vertical)
        {
            root = new GameObject("ConsoleRoot");

            var _scrollRoot = new GameObject("root_");
            var _scroll = _scrollRoot.AddComponent<ScrollRect>();
            GameObject.DontDestroyOnLoad(root);
            var _canvas = root.AddComponent<Canvas>();
            _canvas.gameObject.AddComponent<GraphicRaycaster>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;


            _scrollRoot.transform.SetParent(root.transform, false);
            _scrollRoot.transform.localScale = Vector3.one;
            _scrollRoot.GetComponent<RectTransform>().AnchorTopLeft();
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.inertia = false;
            _scroll.scrollSensitivity = 50;


            cmdcontainer = new GameObject(nameof(CommandLineContainer));
            cmdcontainer.transform.SetParent(_scrollRoot.transform);
            var _cc = cmdcontainer.AddComponent<ContentSizeFitter>();
            _cc.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _cc.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            vertical = cmdcontainer.AddComponent<VerticalLayoutGroup>();
            _scroll.content = cmdcontainer.transform.GetComponent<RectTransform>();

            _scroll.horizontal = false;
            var _handle = new GameObject("console handle").transform;
            _handle.SetParent(_scrollRoot.transform);
            var _handleRect = _handle.AddOrGetComponent<RectTransform>();
            _handleRect.AnchorMiddleLeft();
            _handleRect.sizeDelta = new Vector2(5, 2000);
            _handleRect.anchoredPosition = Vector2.zero;
            _handleRect.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);


            return _canvas;
        }

        protected virtual void configure_layout(HorizontalOrVerticalLayoutGroup layout, bool stretch = false)
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
            apply_padding_spacing_for_layout(layout, out var _padding);


            var _rect = layout.GetComponent<RectTransform>();
            // Apply spacing
            if (stretch)
            {
                _rect.anchorMin = new Vector2(0, 0);
                _rect.anchorMax = new Vector2(1, 1);
                _rect.offsetMin = Vector2.zero;
                _rect.offsetMax = Vector2.zero;
            }

            _rect.localScale = Vector3.one;
        }

        protected virtual void apply_padding_spacing_for_layout(HorizontalOrVerticalLayoutGroup layout,
            out RectOffset _padding)
        {
            _padding = layout.padding;
            _padding.top = 5;
            layout.padding = _padding;
            layout.spacing = 5;
        }


        public static void EnsureEventSystemExists(Canvas parentCanvas = null)
        {
            // already present?
            if (EventSystem.current != null)
                return;

            var _go = new GameObject("EventSystem", typeof(EventSystem));

            // Try to attach the correct input module:
            if (try_add_input_system_ui_module(_go) == false)
                // fallback to old input system
                _go.AddComponent<StandaloneInputModule>();

            if (parentCanvas != null)
                _go.transform.SetParent(parentCanvas.transform, false);

            Object.DontDestroyOnLoad(_go); // optional, keeps it persistent

            Debug.Log("[UIUtils] Created EventSystem with proper input module!");
        }

#if ENABLE_INPUT_SYSTEM
        private static bool try_add_input_system_ui_module(GameObject go)
        {
            // new Input System present
            var _type = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (_type != null)
            {
                go.AddComponent(_type);
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

            var _lines = message.Split(new[] { "\n" }, StringSplitOptions.None);

            foreach (var _line in _lines)
            {
                var _trimmed = _line.Trim();
                if (!string.IsNullOrEmpty(_trimmed)) output.Add(_trimmed);
            }
        }

        public bool Enabled
        {
            set
            {
                if (value) Enable();
                else Disable();
            }
            get => enabled;
        }

        private bool enabled;

        private List<IInputController> controllers_that_were_disabled;

        public void Enable()
        {
            enabled = true;
            Root.gameObject.SetActive(true);


            controllers_that_were_disabled =
                controllerManager.Controllers.Enabled().ToList();
            controllerManager.Controllers.DisableAll();

            CheckIfCommandsDontFit(CommandLineContainer.GetComponent<RectTransform>(),
                Root.GetComponent<RectTransform>());
        }

        public void Disable()
        {
            enabled = false;
            Root.gameObject.SetActive(false);

            controllers_that_were_disabled?.EnableAll();

            CheckIfCommandsDontFit(CommandLineContainer.GetComponent<RectTransform>(),
                Root.GetComponent<RectTransform>());
        }

        public List<ICommandInterpreter> Children { get; } = new List<ICommandInterpreter>();

        List<ICommandInterpreter> IPetOwnerTreeOf<ICommandInterpreter>.Owners { get; } = new();

        List<IDebugSys<IGlobalGizmos, IConsoleMessenger>>
            IPetOf<IDebugSys<IGlobalGizmos, IConsoleMessenger>, IDebugSubSys>.Owners { get; } = new();
    }
}