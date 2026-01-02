using System;
using DAFP.TOOLS.ECS.BigData;
using TNRD;
using UnityEditor;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Editor
{
    /// <summary>
    /// Utilities for drawing lists of SerializableInterface&lt;IStatBase&gt; with deterministic name-based colors.
    /// </summary>
    public static class StatListGUI
    {
        /// <summary>
        /// Draws a single element of a ReorderableList containing SerializableInterface&lt;IStatBase&gt;.
        /// - Background color and a left accent bar are derived from the stat name (stable per name).
        /// - Label shows the stat name; falls back to the stat type or "<null>".
        /// Use this in ReorderableList.drawElementCallback.
        /// </summary>
        public static void DrawSerializableStatElement(Rect rect, SerializedProperty element, int index, bool isActive, bool isFocused)
        {
            // Resolve stat instance
            var stat = GetStatFromSerializableInterfaceProperty(element);

            // Compute display name and color
            string displayName = GetDisplayName(stat, index);
            Color baseColor = ColorFromString(displayName);

            // Calculate full element height (expanded properties supported)
            float height = EditorGUI.GetPropertyHeight(element, includeChildren: true);
            Rect fullRect = new Rect(rect.x, rect.y + 1, rect.width, height + 2);

            // Background and accent colors (tweak for light/dark skins)
            bool pro = EditorGUIUtility.isProSkin;
            var bg = baseColor;
            bg.a = pro ? 0.18f : 0.12f;
            var accent = baseColor;
            accent.a = 1.0f;

            // Draw background
            EditorGUI.DrawRect(fullRect, bg);

            // Draw a small accent bar at the left
            const float accentWidth = 3f;
            var accentRect = new Rect(fullRect.x, fullRect.y, accentWidth, fullRect.height);
            EditorGUI.DrawRect(accentRect, accent);

            // Slight indent for the field to not overlap the accent bar
            var fieldRect = new Rect(rect.x + accentWidth + 2f, rect.y + 2f, rect.width - (accentWidth + 2f), height);

            // Draw the property with our label
            EditorGUI.PropertyField(fieldRect, element, new GUIContent(displayName), true);
        }

        /// <summary>
        /// Returns a deterministic color derived from the input string.
        /// Hue is based on a stable 32-bit FNV-1a hash; saturation/value tuned for readability.
        /// </summary>
        public static Color ColorFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                input = "<null>";

            uint hash = Fnv1a32(input);
            // Map to hue [0,1)
            float h = (hash % 360u) / 360f;
            // Derive S and V with slight variation to spread colors but keep readable
            float s = 0.55f + ((hash >> 8) & 0xFF) / 255f * 0.25f; // 0.55 - 0.80
            float v = EditorGUIUtility.isProSkin ? 0.65f : 0.85f;   // darker in Pro, lighter in Personal

            var color = Color.HSVToRGB(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
            color.a = 1f;
            return color;
        }

        /// <summary>
        /// Extracts the stat instance from a SerializableInterface&lt;IStatBase&gt; SerializedProperty.
        /// </summary>
        public static IStatBase GetStatFromSerializableInterfaceProperty(SerializedProperty element)
        {
            if (element == null) return null;

            var unityRefProp = element.FindPropertyRelative("unityReference");
            if (unityRefProp != null && unityRefProp.objectReferenceValue is IStatBase unityStat)
                return unityStat;

            var rawRefProp = element.FindPropertyRelative("rawReference");
            if (rawRefProp != null && rawRefProp.managedReferenceValue is IStatBase rawStat)
                return rawStat;

            // As a last resort, try using the public Value via reflection if structure changes in future
            // Not strictly necessary now, but provides some forward compatibility.
            try
            {
                var target = element.serializedObject?.targetObject;
                // No easy way to get Value cleanly from here without field access; skip.
            }
            catch { /* ignored */ }

            return null;
        }

        /// <summary>
        /// Composes the display name for a stat.
        /// </summary>
        public static string GetDisplayName(IStatBase stat, int index)
        {
            if (stat == null)
                return $"Element {index} (<null>)";

            if (!string.IsNullOrEmpty(stat.Name))
                return stat.Name;

            return stat.GetType().Name;
        }

        /// <summary>
        /// Stable 32-bit FNV-1a hash for strings.
        /// </summary>
        private static uint Fnv1a32(string input)
        {
            const uint offset = 2166136261;
            const uint prime = 16777619;
            uint hash = offset;
            for (int i = 0; i < input.Length; i++)
            {
                hash ^= input[i];
                hash *= prime;
            }
            return hash;
        }
    }
}
