using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_UI
using UnityEngine.UI;
#endif

/// <summary>
/// Universal color changer for Unity objects.
///
/// WORKS WITH:
///  - SpriteRenderer
///  - MeshRenderer / SkinnedMeshRenderer (via MaterialPropertyBlock, no material instancing)
///  - UI Graphic (Image, RawImage, Text, etc.)
///  - Light
///  - TextMeshPro (TMP_Text) — auto-detected via reflection so no hard dependency needed
///
/// HOW TO USE:
///  1. Drop this script on any GameObject that has one of the above components
///     (children are searched too, so it also works on parent "controller" objects).
///  2. Drag it into the Animation/Animator window and keyframe the "Current Color"
///     field directly — it updates live every frame, both from animation curves
///     AND from script calls.
///  3. Or call the public methods from code: SetColor(), SetWhite(), SetColorHex(),
///     FadeToColor(), FlashColor().
/// </summary>
[DisallowMultipleComponent]
[ExecuteAlways]
public class UniversalColorChanger : MonoBehaviour
{
    [Header("Color (animatable - keyframe this field in the Animation window)")]
    public Color currentColor = Color.white;

    [Header("Renderer material property (only used for 3D Renderers)")]
    public string materialColorProperty = "_Color";

    [Tooltip("If the main property above isn't found, also try this one (useful for URP).")]
    public string materialColorPropertyFallback = "_BaseColor";

    [Header("\"Normal\" color (what SetToNormal() restores)")]
    [Tooltip(
        "If left as default (0,0,0,0), SetToNormal() will use whatever color the object had when it was first enabled.")]
    public Color normalColor = new Color(0, 0, 0, 0);

    [Header("Optional explicit targets (leave empty to auto-detect)")]
    public Renderer[] explicitRenderers;

    public SpriteRenderer[] explicitSpriteRenderers;
#if UNITY_UI
    public Graphic[] explicitGraphics;
#endif
    public Light[] explicitLights;
    public Component[] explicitTMPTexts; // assign TMP_Text components here if you use TextMeshPro

    // --- internals ---
    private Renderer[] _renderers;
    private SpriteRenderer[] _spriteRenderers;
#if UNITY_UI
    private Graphic[] _graphics;
#endif
    private Light[] _lights;
    private Component[] _tmpTexts;
    private MaterialPropertyBlock _mpb;
    private Color _lastApplied;
    private bool _initialized;
    private Color _originalColor;

    private void OnEnable()
    {
        Initialize();
        _originalColor = currentColor;
        Apply(force: true);
    }

    private void Initialize()
    {
        _mpb ??= new MaterialPropertyBlock();
        if (_initialized) return;

        _renderers = (explicitRenderers != null && explicitRenderers.Length > 0)
            ? explicitRenderers
            : GetComponentsInChildren<Renderer>(true);

        _spriteRenderers = (explicitSpriteRenderers != null && explicitSpriteRenderers.Length > 0)
            ? explicitSpriteRenderers
            : GetComponentsInChildren<SpriteRenderer>(true);

#if UNITY_UI
        _graphics = (explicitGraphics != null && explicitGraphics.Length > 0)
            ? explicitGraphics
            : GetComponentsInChildren<Graphic>(true);
#endif

        _lights = (explicitLights != null && explicitLights.Length > 0)
            ? explicitLights
            : GetComponentsInChildren<Light>(true);

        // TextMeshPro support without requiring the package to be installed:
        // we grab any component whose type name is "TextMeshPro" / "TextMeshProUGUI"
        // and set its "color" property via reflection.
        if (explicitTMPTexts != null && explicitTMPTexts.Length > 0)
        {
            _tmpTexts = explicitTMPTexts;
        }
        else
        {
            var all = GetComponentsInChildren<Component>(true);
            var list = new List<Component>();
            foreach (var c in all)
            {
                if (c == null) continue;
                string typeName = c.GetType().Name;
                if (typeName == "TextMeshPro" || typeName == "TextMeshProUGUI" || typeName == "TMP_Text")
                    list.Add(c);
            }

            _tmpTexts = list.ToArray();
        }

        _initialized = true;
    }

    private void LateUpdate()
    {
        if (!_initialized) Initialize();
        if (currentColor != _lastApplied)
            Apply();
    }

    /// <summary>Pushes currentColor to every detected renderer/graphic/light.</summary>
    private void Apply(bool force = false)
    {
        if (!force && currentColor == _lastApplied) return;

        // 3D Renderers (mesh/skinned mesh) via MaterialPropertyBlock — avoids
        // creating material instances, works fine with animation & batching.
        if (_renderers != null)
        {
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                if (HasProperty(r, materialColorProperty))
                    _mpb.SetColor(materialColorProperty, currentColor);
                else if (HasProperty(r, materialColorPropertyFallback))
                    _mpb.SetColor(materialColorPropertyFallback, currentColor);
                r.SetPropertyBlock(_mpb);
            }
        }

        if (_spriteRenderers != null)
            foreach (var sr in _spriteRenderers)
                if (sr != null)
                    sr.color = currentColor;

#if UNITY_UI
        if (_graphics != null)
            foreach (var g in _graphics)
                if (g != null) g.color = currentColor;
#endif

        if (_lights != null)
            foreach (var l in _lights)
                if (l != null)
                    l.color = currentColor;

        if (_tmpTexts != null)
        {
            foreach (var t in _tmpTexts)
            {
                if (t == null) continue;
                var prop = t.GetType().GetProperty("color");
                if (prop != null && prop.CanWrite)
                    prop.SetValue(t, currentColor);
            }
        }

        _lastApplied = currentColor;
    }

    private bool HasProperty(Renderer r, string propName)
    {
        return r.sharedMaterial != null && !string.IsNullOrEmpty(propName) && r.sharedMaterial.HasProperty(propName);
    }

    // ------------------- PUBLIC API -------------------

    /// <summary>Instantly set the color.</summary>
    public void SetColor(Color c)
    {
        currentColor = c;
        Apply();
    }

    /// <summary>Instantly set to pure white.</summary>
    public void SetWhite() => SetColor(Color.white);

    /// <summary>Instantly set to pure black.</summary>
    public void SetBlack() => SetColor(Color.black);

    /// <summary>Set color from a hex string, e.g. "#FF00AA" or "FF00AA".</summary>
    public void SetColorHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out Color c))
            SetColor(c);
        else
            Debug.LogWarning($"UniversalColorChanger: could not parse hex color '{hex}'");
    }

    /// <summary>Smoothly fade to a target color over 'duration' seconds.</summary>
    public Coroutine FadeToColor(Color target, float duration)
    {
        StopAllCoroutines();
        return StartCoroutine(FadeRoutine(target, duration));
    }

    /// <summary>Flash to a color (e.g. white) and back to whatever color it was before the flash.</summary>
    public Coroutine FlashColor(Color flashColor, float duration = 0.15f)
    {
        StopAllCoroutines();
        return StartCoroutine(FlashRoutine(flashColor, duration));
    }

    /// <summary>Convenience: flash pure white, a very common "hit/damage" effect.</summary>
    public Coroutine FlashWhite(float duration = 0.15f) => FlashColor(Color.white, duration);

    private IEnumerator FadeRoutine(Color target, float duration)
    {
        Color start = currentColor;
        float t = 0f;
        if (duration <= 0f)
        {
            SetColor(target);
            yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(start, target, t / duration));
            yield return null;
        }

        SetColor(target);
    }

    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        Color before = currentColor;
        float half = duration * 0.5f;

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(before, flashColor, t / half));
            yield return null;
        }

        SetColor(flashColor);

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(flashColor, before, t / half));
            yield return null;
        }

        SetColor(before);
    }

    /// <summary>Reset to the color this object had when it was first enabled.</summary>
    public void ResetToOriginalColor() => SetColor(_originalColor);

    /// <summary>
    /// Instantly set the object to any full/solid color (alpha included).
    /// Same as SetColor(), named for clarity — e.g. SetFullColor(Color.white),
    /// SetFullColor(Color.red), or SetFullColor(new Color(0f, 1f, 0f)) for full green.
    /// </summary>
    public void SetFullColor(Color c) => SetColor(c);

    /// <summary>Overload: set a full color from RGB (0-1 range), alpha defaults to fully opaque.</summary>
    public void SetFullColor(float r, float g, float b, float a = 1f) => SetColor(new Color(r, g, b, a));

    /// <summary>Overload: set a full color from a hex string, e.g. "#FF00AA".</summary>
    public void SetFullColor(string hex) => SetColorHex(hex);

    /// <summary>
    /// Restore the "normal" color. Uses the 'Normal Color' field if you set one in the
    /// Inspector; otherwise falls back to whatever color the object had on first enable.
    /// </summary>
    public void SetToNormal()
    {
        Color target = (normalColor.a == 0f && normalColor.r == 0f && normalColor.g == 0f && normalColor.b == 0f)
            ? _originalColor
            : normalColor;
        SetColor(target);
    }

    /// <summary>Smoothly fade back to normal over 'duration' seconds (uses the same rule as SetToNormal).</summary>
    public Coroutine FadeToNormal(float duration)
    {
        Color target = (normalColor.a == 0f && normalColor.r == 0f && normalColor.g == 0f && normalColor.b == 0f)
            ? _originalColor
            : normalColor;
        return FadeToColor(target, duration);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keeps the color live-updating in the editor even when not playing.
        Initialize();
        Apply(force: true);
    }
#endif
}