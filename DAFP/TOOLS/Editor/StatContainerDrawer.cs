using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.BigData;
using TNRD;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Editor
{
    [CustomEditor(typeof(StatContainer))]
    public class StatContainerDrawer : UnityEditor.Editor
    {
        private ReorderableList _statsList;

        // Play Mode change tracking
        private Dictionary<string, StatSnapshot> _playModeSnapshot = new();
        private Dictionary<string, StatSnapshot> _exitSnapshot = new();
        private bool _wasInPlayMode;

        private struct StatSnapshot
        {
            public object InternalValue;
            public object MinValue;
            public object MaxValue;
        }

        private void OnEnable()
        {
            var statsProperty = serializedObject.FindProperty("Stats");
            _statsList = new ReorderableList(serializedObject, statsProperty, true, true, true, true);

            _statsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, $"Stats ({statsProperty.arraySize})");
                var btnRect = new Rect(rect.xMax - 60f, rect.y, 60f, rect.height);
                using (new EditorGUI.DisabledScope(statsProperty.arraySize == 0))
                {
                    if (GUI.Button(btnRect, "Clear"))
                    {
                        Undo.RecordObject(serializedObject.targetObject, "Clear Stats");
                        statsProperty.ClearArray();
                        serializedObject.ApplyModifiedProperties();
                        GUI.FocusControl(null);
                    }
                }
            };

            _statsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = statsProperty.GetArrayElementAtIndex(index);
                StatListGUI.DrawSerializableStatElement(rect, element, index, isActive, isFocused);
            };

            _statsList.elementHeightCallback = index =>
            {
                var element = statsProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element) + 4;
            };

            EditorApplication.playModeStateChanged += on_play_mode_state_changed;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= on_play_mode_state_changed;
        }

        private void on_play_mode_state_changed(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    _playModeSnapshot = snapshot_stats();
                    _wasInPlayMode = true;
                    break;

                // Grab current (live) values BEFORE Unity reverts them
                case PlayModeStateChange.ExitingPlayMode when _wasInPlayMode:
                    _exitSnapshot = snapshot_stats();
                    break;

                // Now compare the two snapshots — serialized values are already reverted here
                case PlayModeStateChange.EnteredEditMode when _wasInPlayMode:
                    _wasInPlayMode = false;
                    check_play_mode_changes();
                    break;
            }
        }

        // ── Snapshot ─────────────────────────────────────────────────────────────

        private Dictionary<string, StatSnapshot> snapshot_stats()
        {
            var snap = new Dictionary<string, StatSnapshot>();
            serializedObject.Update();
            var statsProperty = serializedObject.FindProperty("Stats");
            if (statsProperty == null) return snap;

            for (int i = 0; i < statsProperty.arraySize; i++)
                collect_snapshots(statsProperty.GetArrayElementAtIndex(i), snap);

            return snap;
        }

        private static void collect_snapshots(SerializedProperty prop, Dictionary<string, StatSnapshot> snap)
        {
            if (prop == null) return;

            var intVal = find_prop(prop, "InternalValue");
            var minVal = find_prop(prop, "MinValue");
            var maxVal = find_prop(prop, "MaxValue");

            if (intVal != null || minVal != null || maxVal != null)
            {
                snap[prop.propertyPath] = new StatSnapshot
                {
                    InternalValue = read_value(intVal),
                    MinValue = read_value(minVal),
                    MaxValue = read_value(maxVal),
                };
            }

            // Recurse into children
            var children = prop.FindPropertyRelative(nameof(WhiteBoard<uint>.ChildrenStats));
            if (children == null || !children.isArray) return;
            for (int i = 0; i < children.arraySize; i++)
                collect_snapshots(children.GetArrayElementAtIndex(i), snap);
        }

        // ── Change detection & dialog ─────────────────────────────────────────────

        private void check_play_mode_changes()
        {
            if (_exitSnapshot.Count == 0)
            {
                _playModeSnapshot.Clear();
                return;
            }

            bool anyChanged = false;
            foreach (var kvp in _exitSnapshot)
            {
                if (!_playModeSnapshot.TryGetValue(kvp.Key, out var before)) continue;
                if (!Equals(before.InternalValue, kvp.Value.InternalValue) ||
                    !Equals(before.MinValue, kvp.Value.MinValue) ||
                    !Equals(before.MaxValue, kvp.Value.MaxValue))
                {
                    anyChanged = true;
                    break;
                }
            }

            if (!anyChanged)
            {
                _playModeSnapshot.Clear();
                _exitSnapshot.Clear();
                return;
            }

            bool keep = EditorUtility.DisplayDialog(
                "Keep Play Mode Changes?",
                "Stat values were changed manually during Play Mode.\n\nApply them to the original ScriptableObject?",
                "Keep", "Discard");

            if (keep)
                apply_to_source_asset(_exitSnapshot); // use exit snapshot, not reverted values

            _playModeSnapshot.Clear();
            _exitSnapshot.Clear();
        }

        // ── Apply to source SO ────────────────────────────────────────────────────

        private void apply_to_source_asset(Dictionary<string, StatSnapshot> current)
        {
            // Strip Unity's clone suffix to find the original asset name
            string cloneName = serializedObject.targetObject.name;
            string assetName = strip_clone_suffix(cloneName);

            // Search the whole project for a StatContainer SO with that name
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(StatContainer)} {assetName}");
            StatContainer sourceAsset = null;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var candidate = AssetDatabase.LoadAssetAtPath<StatContainer>(path);
                if (candidate != null && candidate.name == assetName)
                {
                    sourceAsset = candidate;
                    break;
                }
            }

            if (sourceAsset == null)
            {
                Debug.LogWarning($"[StatContainerDrawer] Could not find source asset '{assetName}' in project.");
                return;
            }

            var sourceSO = new SerializedObject(sourceAsset);
            sourceSO.Update();
            var sourceStats = sourceSO.FindProperty("Stats");
            if (sourceStats == null) return;

            // Walk source stats and patch values from our snapshot
            for (int i = 0; i < sourceStats.arraySize; i++)
                patch_stat(sourceStats.GetArrayElementAtIndex(i), current);

            sourceSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(sourceAsset);
            AssetDatabase.SaveAssetIfDirty(sourceAsset);

            Debug.Log($"[StatContainerDrawer] Applied Play Mode changes to '{assetName}'.");
        }

        private static void patch_stat(SerializedProperty sourceProp, Dictionary<string, StatSnapshot> snapshots)
        {
            if (sourceProp == null) return;

            // Match by path suffix — clone paths differ by root object name, so match on everything after the first dot
            string matchKey = null;
            foreach (var key in snapshots.Keys)
            {
                if (paths_match(key, sourceProp.propertyPath))
                {
                    matchKey = key;
                    break;
                }
            }

            if (matchKey != null)
            {
                var snap = snapshots[matchKey];
                write_value(find_prop(sourceProp, "InternalValue"), snap.InternalValue);
                write_value(find_prop(sourceProp, "MinValue"), snap.MinValue);
                write_value(find_prop(sourceProp, "MaxValue"), snap.MaxValue);
            }

            // Recurse into children
            var children = sourceProp.FindPropertyRelative(nameof(WhiteBoard<uint>.ChildrenStats));
            if (children == null || !children.isArray) return;
            for (int i = 0; i < children.arraySize; i++)
                patch_stat(children.GetArrayElementAtIndex(i), snapshots);
        }

        // ── Utilities ─────────────────────────────────────────────────────────────

        // Clone paths look like "Stats.Array.data[0]..." and source paths look the same
        // but rooted on a different object — so we compare everything after the first segment
        private static bool paths_match(string clonePath, string sourcePath)
        {
            int cloneDot = clonePath.IndexOf('.');
            int sourceDot = sourcePath.IndexOf('.');
            if (cloneDot < 0 || sourceDot < 0) return clonePath == sourcePath;
            return clonePath.Substring(cloneDot) == sourcePath.Substring(sourceDot);
        }

        private static string strip_clone_suffix(string name)
        {
            // Unity appends " (Clone)" for Instantiate, but SOs cloned via Instantiate
            // also get that. Strip any trailing " (Clone)" or " (Clone)(Clone)" etc.
            const string suffix = " (Clone)";
            while (name.EndsWith(suffix, StringComparison.Ordinal))
                name = name.Substring(0, name.Length - suffix.Length);
            return name;
        }

        private static SerializedProperty find_prop(SerializedProperty parent, string name)
        {
            if (parent == null) return null;
            return parent.FindPropertyRelative(name)
                   ?? parent.FindPropertyRelative($"<{name}>k__BackingField");
        }

        private static object read_value(SerializedProperty p)
        {
            if (p == null) return null;
            return p.propertyType switch
            {
                SerializedPropertyType.Float => (object)p.floatValue,
#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
                SerializedPropertyType.Integer => (object)p.longValue,
#else
                SerializedPropertyType.Integer => (object)(long)p.intValue,
#endif
                _ => null
            };
        }

        private static void write_value(SerializedProperty p, object value)
        {
            if (p == null || value == null) return;
            switch (p.propertyType)
            {
                case SerializedPropertyType.Float:
                    p.floatValue = Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.Integer:
#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
                    p.longValue = Convert.ToInt64(value);
#else
                    p.intValue = Convert.ToInt32(value);
#endif
                    break;
            }
        }

        // ── Inspector ─────────────────────────────────────────────────────────────

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "Stats");
            EditorGUILayout.Space();
            _statsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}