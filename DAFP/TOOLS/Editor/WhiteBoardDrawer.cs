using System;
using System.Collections.Generic;
using System.Reflection;
using DAFP.TOOLS.ECS.BigData;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RapidLib.DAFP.TOOLS.Editor
{
    [CustomPropertyDrawer(typeof(WhiteBoard<>), true)]
    public class WhiteBoardDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> _lists = new();

        private string ChildrenListName => nameof(WhiteBoard<uint>.ChildrenStats);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float y = position.y, x = position.x, width = position.width;
            bool isChild = is_child_of_another_stat(property);

            property.isExpanded = EditorGUI.Foldout(
                new Rect(x, y, width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                if (isChild)
                {
                    y = draw_separator_line(x, y, width);
                    draw_additional_child_properties(ref y, x, width, property);
                    y = draw_separator_line(x, y, width);
                }

                var iterator = property.Copy();
                var end = iterator.GetEndProperty();
                int targetDepth = property.depth + 1;

                if (iterator.NextVisible(true))
                {
                    while (!SerializedProperty.EqualContents(iterator, end))
                    {
                        if (iterator.depth == targetDepth &&
                            iterator.name != ChildrenListName &&
                            iterator.name != "PegModifiers")
                        {
                            float h = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                            EditorGUI.PropertyField(new Rect(x, y, width, h), iterator, true);
                            y += h + EditorGUIUtility.standardVerticalSpacing;
                        }

                        if (!iterator.NextVisible(false)) break;
                    }
                }

                enforce_numeric_constraints(property);

                var childrenProp = property.FindPropertyRelative(ChildrenListName);
                y += draw_children_list(new Rect(x, y, width, 0), property, childrenProp)
                     + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (!property.isExpanded) return height;

            bool isChild = is_child_of_another_stat(property);

            if (isChild)
            {
                height += (EditorGUIUtility.singleLineHeight * 0.1f + EditorGUIUtility.standardVerticalSpacing) * 2f;
                var peg = property.FindPropertyRelative("PegModifiers");
                if (peg != null)
                    height += EditorGUI.GetPropertyHeight(peg, includeChildren: true)
                              + EditorGUIUtility.standardVerticalSpacing;
            }

            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            int targetDepth = property.depth + 1;

            if (iterator.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(iterator, end))
                {
                    if (iterator.depth == targetDepth &&
                        iterator.name != ChildrenListName &&
                        iterator.name != "PegModifiers")
                    {
                        height += EditorGUI.GetPropertyHeight(iterator, includeChildren: true)
                                  + EditorGUIUtility.standardVerticalSpacing;
                    }

                    if (!iterator.NextVisible(false)) break;
                }
            }

            var childrenProp = property.FindPropertyRelative(ChildrenListName);
            height += get_children_list_height(property, childrenProp) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        // ── Constraints ──────────────────────────────────────────────────────────

        private void enforce_numeric_constraints(SerializedProperty board)
        {
            if (board == null) return;

            var minProp = find_by_possible_names(board, "MinValue");
            var maxProp = find_by_possible_names(board, "MaxValue");
            var intValue = find_by_possible_names(board, "InternalValue");

            if (minProp == null || maxProp == null || intValue == null)
            {
                try_reflection_constraints(board, minProp, maxProp, intValue);
                return;
            }

            bool changed = false;

            if (minProp.propertyType == SerializedPropertyType.Float)
            {
                changed = enforce_float(minProp, maxProp, intValue);
                sync_edit_mode_copies_float(board, intValue, ref changed);
            }
            else if (minProp.propertyType == SerializedPropertyType.Integer)
            {
                changed = enforce_int(minProp, maxProp, intValue);
                sync_edit_mode_copies_int(board, intValue, ref changed);
            }
            else
            {
                try_reflection_constraints(board, minProp, maxProp, intValue);
                return;
            }

            if (changed)
                board.serializedObject.ApplyModifiedProperties();
        }

        private static bool enforce_float(SerializedProperty minP, SerializedProperty maxP, SerializedProperty valP)
        {
            float min = minP.floatValue, max = maxP.floatValue, val = valP.floatValue;
            if (Mathf.Approximately(min, 0f) && Mathf.Approximately(max, 0f) && Mathf.Approximately(val, 0f))
                return false;

            bool changed = false;
            if (max < min)
            {
                (min, max) = (max, min);
                changed = true;
            }

            if (Mathf.Approximately(max, min))
            {
                max = min + Mathf.Max(1e-6f, Mathf.Abs(min) * 1e-6f);
                changed = true;
            }

            float clamped = Mathf.Clamp(val, min, max);
            if (!Mathf.Approximately(clamped, val))
            {
                val = clamped;
                changed = true;
            }

            if (changed)
            {
                minP.floatValue = min;
                maxP.floatValue = max;
                valP.floatValue = val;
            }

            return changed;
        }

        private static bool enforce_int(SerializedProperty minP, SerializedProperty maxP, SerializedProperty valP)
        {
#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
            long min = minP.longValue, max = maxP.longValue, val = valP.longValue;
#else
            long min = minP.intValue, max = maxP.intValue, val = valP.intValue;
#endif
            if (min == 0 && max == 0 && val == 0) return false;

            bool changed = false;
            if (max < min)
            {
                (min, max) = (max, min);
                changed = true;
            }

            if (max == min)
            {
                max = min + 1;
                changed = true;
            }

            long clamped = val < min ? min : val > max ? max : val;
            if (clamped != val)
            {
                val = clamped;
                changed = true;
            }

            if (changed)
            {
#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
                minP.longValue = min;
                maxP.longValue = max;
                valP.longValue = val;
#else
                minP.intValue = (int)min; maxP.intValue = (int)max; valP.intValue = (int)val;
#endif
            }

            return changed;
        }

        // ── Edit-mode copies sync (DefaultValue + RealValue) ─────────────────────

        private static void sync_edit_mode_copies_float(SerializedProperty board, SerializedProperty intP,
            ref bool changed)
        {
            if (EditorApplication.isPlaying) return;

            var defP = find_by_possible_names(board, "DefaultValue");
            var realP = find_by_possible_names(board, "RealValue");

            if (defP != null && defP.propertyType == SerializedPropertyType.Float &&
                !Mathf.Approximately(defP.floatValue, intP.floatValue))
            {
                defP.floatValue = intP.floatValue;
                changed = true;
            }

            if (realP != null && realP.propertyType == SerializedPropertyType.Float &&
                !Mathf.Approximately(realP.floatValue, intP.floatValue))
            {
                realP.floatValue = intP.floatValue;
                changed = true;
            }
        }

        private static void sync_edit_mode_copies_int(SerializedProperty board, SerializedProperty intP,
            ref bool changed)
        {
            if (EditorApplication.isPlaying) return;

            var defP = find_by_possible_names(board, "DefaultValue");
            var realP = find_by_possible_names(board, "RealValue");

#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
            if (defP != null && defP.propertyType == SerializedPropertyType.Integer &&
                defP.longValue != intP.longValue)
            {
                defP.longValue = intP.longValue;
                changed = true;
            }

            if (realP != null && realP.propertyType == SerializedPropertyType.Integer &&
                realP.longValue != intP.longValue)
            {
                realP.longValue = intP.longValue;
                changed = true;
            }
#else
            if (defP != null && defP.propertyType == SerializedPropertyType.Integer &&
                defP.intValue != intP.intValue)
            {
                defP.intValue = intP.intValue;
                changed = true;
            }

            if (realP != null && realP.propertyType == SerializedPropertyType.Integer &&
                realP.intValue != intP.intValue)
            {
                realP.intValue = intP.intValue;
                changed = true;
            }
#endif
        }

        // ── Reflection fallback ───────────────────────────────────────────────────

        private static void try_reflection_constraints(SerializedProperty board,
            SerializedProperty minProp, SerializedProperty maxProp, SerializedProperty defProp)
        {
#if UNITY_2020_1_OR_NEWER
            object instance = board.managedReferenceValue;
            if (instance == null) return;

            Type t = instance.GetType(), whiteboardGeneric = null;
            for (var cur = t; cur != null; cur = cur.BaseType)
            {
                if (cur.IsGenericType && cur.GetGenericTypeDefinition().Name.StartsWith("WhiteBoard"))
                {
                    whiteboardGeneric = cur;
                    break;
                }
            }

            if (whiteboardGeneric == null) return;

            Type valueType = whiteboardGeneric.GetGenericArguments()[0];
            if (!typeof(IComparable).IsAssignableFrom(valueType)) return;

            const BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var minPi = t.GetProperty("MinValue", bf);
            var maxPi = t.GetProperty("MaxValue", bf);
            var defPi = t.GetProperty("InternalValue", bf);
            if (minPi == null || maxPi == null || defPi == null) return;

            object min = minPi.GetValue(instance), max = maxPi.GetValue(instance), def = defPi.GetValue(instance);
            object zero = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
            if (Equals(min, zero) && Equals(max, zero) && Equals(def, zero)) return;

            bool changed = false;
            var cmp = (IComparable)max;

            if (cmp.CompareTo(min) < 0)
            {
                (min, max) = (max, min);
                changed = true;
            }
            else if (cmp.CompareTo(min) == 0 && is_numeric(valueType, out var stepKind))
            {
                max = numeric_add(min, numeric_step(min, stepKind), stepKind);
                changed = true;
            }

            if (is_numeric(valueType, out _))
            {
                if (((IComparable)def).CompareTo(min) < 0)
                {
                    def = min;
                    changed = true;
                }
                else if (((IComparable)def).CompareTo(max) > 0)
                {
                    def = max;
                    changed = true;
                }
            }

            if (changed)
            {
                minPi.SetValue(instance, min);
                maxPi.SetValue(instance, max);
                defPi.SetValue(instance, def);
            }

            // Sync DefaultValue and RealValue to InternalValue in Edit Mode
            if (!EditorApplication.isPlaying)
            {
                object current = defPi.GetValue(instance);

                var defltPi = t.GetProperty("DefaultValue", bf);
                var realPi = t.GetProperty("RealValue", bf);

                if (defltPi != null && defltPi.CanWrite && !Equals(defltPi.GetValue(instance), current))
                {
                    defltPi.SetValue(instance, current);
                    changed = true;
                }

                if (realPi != null && realPi.CanWrite && !Equals(realPi.GetValue(instance), current))
                {
                    realPi.SetValue(instance, current);
                    changed = true;
                }
            }

            if (changed)
            {
                board.serializedObject.ApplyModifiedProperties();
                if (board.serializedObject.targetObject is Object target)
                    EditorUtility.SetDirty(target);
            }
#endif
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static SerializedProperty find_by_possible_names(SerializedProperty parent, string name)
        {
            if (parent == null) return null;
            return parent.FindPropertyRelative(name)
                   ?? parent.FindPropertyRelative($"<{name}>k__BackingField");
        }

        private bool is_child_of_another_stat(SerializedProperty property) =>
            property.propertyPath.Contains($".{ChildrenListName}.Array.data[");

        private static float draw_separator_line(float x, float y, float width)
        {
            float h = EditorGUIUtility.singleLineHeight * 0.1f;
            EditorGUI.DrawRect(new Rect(x, y, width, h), Color.crimson);
            return y + h + EditorGUIUtility.standardVerticalSpacing;
        }

        private void draw_additional_child_properties(ref float y, float x, float width, SerializedProperty property)
        {
            var peg = property.FindPropertyRelative("PegModifiers");
            float h = EditorGUI.GetPropertyHeight(peg, includeChildren: true);
            EditorGUI.PropertyField(new Rect(x, y, width, h), peg, true);
            y += h + EditorGUIUtility.standardVerticalSpacing;
        }

        // ── ReorderableList ───────────────────────────────────────────────────────

        private float draw_children_list(Rect position, SerializedProperty owner, SerializedProperty childrenProp)
        {
            if (childrenProp == null)
            {
                float h = EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, h),
                    "Children list not found.", MessageType.Info);
                return h;
            }

            var list = get_or_create_list(owner, childrenProp);
            float height = list.GetHeight();
            list.DoList(new Rect(position.x, position.y, position.width, height));
            return height;
        }

        private float get_children_list_height(SerializedProperty owner, SerializedProperty childrenProp)
        {
            if (childrenProp == null) return EditorGUIUtility.singleLineHeight * 2f;
            return get_or_create_list(owner, childrenProp).GetHeight();
        }

        private ReorderableList get_or_create_list(SerializedProperty owner, SerializedProperty childrenProp)
        {
            string key = owner.propertyPath + $".{ChildrenListName}";
            if (_lists.TryGetValue(key, out var existing)) return existing;

            var list = new ReorderableList(childrenProp.serializedObject, childrenProp, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                int count = childrenProp.isArray ? childrenProp.arraySize : 0;
                EditorGUI.LabelField(rect, $"Children ({count})");
                var btnRect = new Rect(rect.xMax - 60f, rect.y, 60f, rect.height);
                using (new EditorGUI.DisabledScope(count == 0))
                {
                    if (GUI.Button(btnRect, "Clear"))
                    {
                        Undo.RecordObject(childrenProp.serializedObject.targetObject, "Clear Children");
                        childrenProp.ClearArray();
                        childrenProp.serializedObject.ApplyModifiedProperties();
                        GUI.FocusControl(null);
                    }
                }
            };

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
                StatListGUI.DrawSerializableStatElement(rect, childrenProp.GetArrayElementAtIndex(index), index,
                    isActive, isFocused);

            list.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(childrenProp.GetArrayElementAtIndex(index), includeChildren: true) + 4f;

            _lists[key] = list;
            return list;
        }

        // ── Numeric reflection utils ──────────────────────────────────────────────

        private enum NumericKind
        {
            FloatLike,
            IntLike
        }

        private static bool is_numeric(Type t, out NumericKind kind)
        {
            if (t == typeof(float) || t == typeof(double))
            {
                kind = NumericKind.FloatLike;
                return true;
            }

            if (t == typeof(int) || t == typeof(long) || t == typeof(short) ||
                t == typeof(uint) || t == typeof(ulong) || t == typeof(ushort) ||
                t == typeof(byte) || t == typeof(sbyte))
            {
                kind = NumericKind.IntLike;
                return true;
            }

            kind = NumericKind.IntLike;
            return false;
        }

        private static object numeric_step(object min, NumericKind kind)
        {
            if (kind == NumericKind.FloatLike)
            {
                double v = Convert.ToDouble(min);
                return Math.Max(Math.Abs(v) * 1e-6, 1e-6);
            }

            return 1L;
        }

        private static object numeric_add(object a, object b, NumericKind kind) =>
            kind == NumericKind.FloatLike
                ? (object)(Convert.ToDouble(a) + Convert.ToDouble(b))
                : Convert.ToInt64(a) + Convert.ToInt64(b);
    }
}