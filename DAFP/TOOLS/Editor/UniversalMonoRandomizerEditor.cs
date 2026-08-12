using RapidLib.DAFP.TOOLS.Common;

namespace RapidLib.DAFP.TOOLS.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using RapidLib.DAFP.TOOLS.Common.Randomization;

    namespace RapidLib.DAFP.TOOLS.Common.Randomization.Editor
    {
        [CustomEditor(typeof(UniversalComponentRandomizer))]
        public class ScriptRandomizerEditor : UnityEditor.Editor
        {
            private const BindingFlags FieldFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            private readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();

            public override void OnInspectorGUI()
            {
                var randomizer = (UniversalComponentRandomizer)target;

                EditorGUI.BeginChangeCheck();
                var newTarget = (Component)EditorGUILayout.ObjectField(
                    "Target", randomizer.target, typeof(Component), true);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(randomizer, "Change Randomizer Target");
                    randomizer.target = newTarget;
                    EditorUtility.SetDirty(randomizer);
                }

                if (randomizer.target == null)
                {
                    EditorGUILayout.HelpBox(
                        "Assign a MonoBehaviour (or Transform) to pick which of its fields to randomize.",
                        MessageType.Info);
                    return;
                }

                EditorGUILayout.Space();
                DrawFieldsRecursive(randomizer, randomizer.target, randomizer.target.GetType(), "", "");
            }

            private void DrawFieldsRecursive(UniversalComponentRandomizer randomizer, object obj, Type type,
                string pathPrefix,
                string labelPrefix)
            {
                foreach (var member in GetSerializedMembers(type))
                {
                    string path = string.IsNullOrEmpty(pathPrefix) ? member.Name : $"{pathPrefix}.{member.Name}";
                    string label = string.IsNullOrEmpty(labelPrefix) ? member.Name : $"{labelPrefix}/{member.Name}";
                    var memberType = member.MemberType;

                    if (FieldKindUtility.IsSupportedLeaf(memberType))
                    {
                        DrawLeafField(randomizer, path, label, memberType);
                    }
                    else if (FieldKindUtility.IsNestedSerializable(memberType))
                    {
                        _foldouts.TryGetValue(path, out bool open);
                        open = EditorGUILayout.Foldout(open, member.Name, true);
                        _foldouts[path] = open;

                        if (open)
                        {
                            EditorGUI.indentLevel++;
                            object nestedValue = member.GetValue(obj) ?? Activator.CreateInstance(memberType);
                            DrawFieldsRecursive(randomizer, nestedValue, memberType, path, label);
                            EditorGUI.indentLevel--;
                        }
                    }
                    else
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.LabelField(member.Name, $"(unsupported type: {memberType.Name})");
                        }
                    }
                }
            }

            private void DrawLeafField(UniversalComponentRandomizer randomizer, string path, string label,
                Type fieldType)
            {
                var setting = GetOrCreateSetting(randomizer, path, label, fieldType);
                var kind = FieldKindUtility.GetKind(fieldType);
                bool isWideKind = kind == FieldKind.Vector2 || kind == FieldKind.Vector3 ||
                                  kind == FieldKind.Vector4 || kind == FieldKind.Color;

                EditorGUILayout.BeginHorizontal();
                bool enabled = EditorGUILayout.ToggleLeft(label, setting.enabled, GUILayout.Width(200));
                if (enabled != setting.enabled)
                {
                    Undo.RecordObject(randomizer, "Toggle Randomize Field");
                    setting.enabled = enabled;
                    EditorUtility.SetDirty(randomizer);
                }

                if (!isWideKind)
                {
                    using (new EditorGUI.DisabledScope(!setting.enabled))
                    {
                        DrawRangeControls(randomizer, setting, fieldType);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (isWideKind)
                {
                    using (new EditorGUI.DisabledScope(!setting.enabled))
                    {
                        EditorGUI.indentLevel++;
                        DrawRangeControls(randomizer, setting, fieldType);
                        EditorGUI.indentLevel--;
                    }
                }
            }

            private void DrawRangeControls(UniversalComponentRandomizer randomizer, FieldRandomSetting setting,
                Type fieldType)
            {
                EditorGUI.BeginChangeCheck();

                switch (FieldKindUtility.GetKind(fieldType))
                {
                    case FieldKind.Float:
                        setting.minFloat = EditorGUILayout.FloatField(setting.minFloat, GUILayout.Width(60));
                        EditorGUILayout.LabelField("to", GUILayout.Width(20));
                        setting.maxFloat = EditorGUILayout.FloatField(setting.maxFloat, GUILayout.Width(60));
                        break;

                    case FieldKind.Int:
                        setting.minInt = EditorGUILayout.IntField(setting.minInt, GUILayout.Width(60));
                        EditorGUILayout.LabelField("to", GUILayout.Width(20));
                        setting.maxInt = EditorGUILayout.IntField(setting.maxInt, GUILayout.Width(60));
                        break;

                    case FieldKind.UInt:
                        int minAsInt = EditorGUILayout.IntField((int)setting.minUInt, GUILayout.Width(60));
                        setting.minUInt = (uint)Mathf.Max(0, minAsInt);
                        EditorGUILayout.LabelField("to", GUILayout.Width(20));
                        int maxAsInt = EditorGUILayout.IntField((int)setting.maxUInt, GUILayout.Width(60));
                        setting.maxUInt = (uint)Mathf.Max(0, maxAsInt);
                        break;

                    case FieldKind.Bool:
                        EditorGUILayout.LabelField("Chance:", GUILayout.Width(50));
                        setting.boolChance = EditorGUILayout.Slider(setting.boolChance, 0f, 1f);
                        break;

                    case FieldKind.Vector2:
                        setting.minVector = EditorGUILayout.Vector2Field("Min", setting.minVector);
                        setting.maxVector = EditorGUILayout.Vector2Field("Max", setting.maxVector);
                        break;

                    case FieldKind.Vector3:
                        setting.minVector = EditorGUILayout.Vector3Field("Min", setting.minVector);
                        setting.maxVector = EditorGUILayout.Vector3Field("Max", setting.maxVector);
                        break;

                    case FieldKind.Vector4:
                        setting.minVector = EditorGUILayout.Vector4Field("Min", setting.minVector);
                        setting.maxVector = EditorGUILayout.Vector4Field("Max", setting.maxVector);
                        break;

                    case FieldKind.Color:
                        setting.minColor = EditorGUILayout.ColorField("Min", setting.minColor);
                        setting.maxColor = EditorGUILayout.ColorField("Max", setting.maxColor);
                        break;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(randomizer, "Edit Randomize Range");
                    EditorUtility.SetDirty(randomizer);
                }
            }

            private FieldRandomSetting GetOrCreateSetting(UniversalComponentRandomizer randomizer, string path,
                string label,
                Type fieldType)
            {
                foreach (var s in randomizer.settings)
                {
                    if (s.path == path) return s;
                }

                var newSetting = new FieldRandomSetting
                {
                    path = path,
                    displayName = label,
                    fieldTypeAssemblyQualifiedName = fieldType.AssemblyQualifiedName,
                    enabled = false
                };

                Undo.RecordObject(randomizer, "Add Randomize Field");
                randomizer.settings.Add(newSetting);
                EditorUtility.SetDirty(randomizer);

                return newSetting;
            }

            // Walks up the inheritance chain so private [SerializeField] fields
            // declared on base classes are included, same as Unity's own serializer.
            // Also injects a curated set of settable properties for types (like
            // Transform) whose real data isn't reachable as normal fields.
            private static IEnumerable<MemberAccessor> GetSerializedMembers(Type type)
            {
                var t = type;
                while (t != null && t != typeof(MonoBehaviour) && t != typeof(object))
                {
                    foreach (var field in t.GetFields(FieldFlags | BindingFlags.DeclaredOnly))
                    {
                        if (field.IsStatic) continue;
                        if (Attribute.IsDefined(field, typeof(NonSerializedAttribute))) continue;

                        bool serialized = field.IsPublic ||
                                          Attribute.IsDefined(field, typeof(SerializeField));
                        if (!serialized) continue;

                        yield return new MemberAccessor(field);
                    }

                    t = t.BaseType;
                }

                if (TransformMemberWhitelist.AppliesTo(type))
                {
                    foreach (var prop in TransformMemberWhitelist.GetWhitelistedProperties())
                    {
                        yield return new MemberAccessor(prop);
                    }
                }
            }
        }
    }
}