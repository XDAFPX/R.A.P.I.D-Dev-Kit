using System;
using System.Collections.Generic;
using System.Reflection;
using DAFP.TOOLS.ECS.BigData;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Editor
{
    /// <summary>
    /// Drawer for any WhiteBoard<> derived type.
    /// Ensures the list of Children is always drawn at the very end, using StatListGUI styling.
    /// Other fields are drawn in their normal order before the Children list.
    /// </summary>
    [CustomPropertyDrawer(typeof(WhiteBoard<>), true)]
    public class WhiteBoardDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property scope
            EditorGUI.BeginProperty(position, label, property);

            // Check if this WhiteBoard is a child of another stat
            bool _isChildStat = is_child_of_another_stat(property);

            // Calculate rects for fields
            float _y = position.y;
            float _x = position.x;
            float _width = position.width;

            // Draw foldout for the root (managed reference types often show a foldout)
            property.isExpanded = EditorGUI.Foldout(new Rect(_x, _y, _width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);
            _y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Show info box if this is a child stat
                if (_isChildStat)
                {
                    _y = draw_separator_line(_x, _y, _width);
                    draw_additional_child_properties(ref _y, _x, _width, property);
                    _y = draw_separator_line(_x, _y, _width);
                }


                // Iterate direct children excluding 'Children'
                var _iterator = property.Copy();
                var _end = _iterator.GetEndProperty();

                // Move to first visible child
                bool _hasChild = _iterator.NextVisible(true);
                while (_hasChild && !SerializedProperty.EqualContents(_iterator, _end))
                {
                    // Only draw direct children of this property
                    if (_iterator.depth == property.depth + 1 && _iterator.name != "Children" &&
                        _iterator.name != "PegModifiers")
                    {
                        float _h = EditorGUI.GetPropertyHeight(_iterator, includeChildren: true);
                        EditorGUI.PropertyField(new Rect(_x, _y, _width, _h), _iterator, true);
                        _y += _h + EditorGUIUtility.standardVerticalSpacing;
                    }

                    _hasChild = _iterator.NextVisible(false);
                }

                // Draw additional properties for child stats

                // Before drawing Children, enforce numeric constraints if applicable
                enforce_numeric_constraints(property);

                // Draw Children list at the end
                var _childrenProp = property.FindPropertyRelative("Children");
                float _listHeight = draw_children_list(new Rect(_x, _y, _width, 0), property, _childrenProp);
                _y += _listHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private static float draw_separator_line(float _x, float _y, float _width)
        {
            float _separatorH = EditorGUIUtility.singleLineHeight * 0.1f;
            EditorGUI.DrawRect(new Rect(_x, _y, _width, _separatorH), Color.crimson);
            _y += _separatorH + EditorGUIUtility.standardVerticalSpacing;
            return _y;
        }

        /// <summary>
        /// Checks if this property is inside a "Children" array of another stat
        /// </summary>
        private bool is_child_of_another_stat(SerializedProperty property)
        {
            // Check if the property path contains ".Children.Array.data["
            // This indicates it's an element within a Children list
            return property.propertyPath.Contains(".Children.Array.data[");
        }

        /// <summary>
        /// Draw additional properties that should only appear for child stats
        /// </summary>
        private void draw_additional_child_properties(ref float y, float x, float width, SerializedProperty property)
        {
            var _iterator = property.FindPropertyRelative("PegModifiers");
            float _h = EditorGUI.GetPropertyHeight(_iterator, includeChildren: true);
            EditorGUI.PropertyField(new Rect(x, y, width, _h), _iterator, true);
            y += _h + EditorGUIUtility.standardVerticalSpacing;
        }

        // Try normal field name and auto-property backing field name
        private static SerializedProperty find_by_possible_names(SerializedProperty parent, string baseName)
        {
            if (parent == null) return null;
            // 1) direct field name
            var p = parent.FindPropertyRelative(baseName);
            if (p != null) return p;
            // 2) auto-property backing field generated by compiler: <Name>k__BackingField
            string backing = "<" + baseName + ">k__BackingField";
            p = parent.FindPropertyRelative(backing);
            if (p != null) return p;
            return null;
        }

        /// <summary>
        /// Enforce numeric constraints for WhiteBoard<T> where T is a numeric/comparable type.
        /// - Ensure MaxValue > MinValue (strict).
        /// - Ensure MinValue < MaxValue (strict).
        /// - Ensure DefaultValue is clamped within [MinValue, MaxValue].
        /// - Skip entirely if all three values equal default(T) (uninitialized state).
        /// </summary>
        private void enforce_numeric_constraints(SerializedProperty boardProperty)
        {
            if (boardProperty == null)
                return;

            // Access sub-properties (support auto-property backing fields)
            var minProp = find_by_possible_names(boardProperty, "MinValue");
            var maxProp = find_by_possible_names(boardProperty, "MaxValue");
            var defProp = find_by_possible_names(boardProperty, "DefaultValue");

            // If we cannot access via SerializedProperty, try reflection path
            if (minProp == null || maxProp == null || defProp == null)
            {
                try_reflection_constraints(boardProperty, minProp, maxProp, defProp);
                return;
            }
            
            // Only handle primitive numeric types Unity exposes directly
            // Note: For managed references, numeric T appears as Integer or Float
            if (minProp.propertyType == SerializedPropertyType.Float &&
                maxProp.propertyType == SerializedPropertyType.Float &&
                defProp.propertyType == SerializedPropertyType.Float)
            {
                float min = minProp.floatValue;
                float max = maxProp.floatValue;
                float def = defProp.floatValue;

                // Skip if all are default(T) == 0
                if (Mathf.Approximately(min, 0f) && Mathf.Approximately(max, 0f) && Mathf.Approximately(def, 0f))
                    return;

                bool changed = false;

                // Order enforcement
                if (max < min)
                {
                    // swap
                    (min, max) = (max, min);
                    changed = true;
                }
                if (Mathf.Approximately(max, min))
                {
                    // make strictly greater using a tiny epsilon
                    float step = Mathf.Max(1e-6f, Mathf.Abs(min) * 1e-6f);
                    max = min + step;
                    changed = true;
                }

                // Clamp default
                float clampedDef = Mathf.Clamp(def, min, max);
                if (!Mathf.Approximately(clampedDef, def))
                {
                    def = clampedDef;
                    changed = true;
                }

                if (changed)
                {
                    minProp.floatValue = min;
                    maxProp.floatValue = max;
                    defProp.floatValue = def;
                    boardProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            else if (minProp.propertyType == SerializedPropertyType.Integer &&
                     maxProp.propertyType == SerializedPropertyType.Integer &&
                     defProp.propertyType == SerializedPropertyType.Integer)
            {
                // Use long to support large ranges; Unity maps many ints to longValue in newer versions
#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
                long min = minProp.longValue;
                long max = maxProp.longValue;
                long def = defProp.longValue;
#else
                long min = minProp.intValue;
                long max = maxProp.intValue;
                long def = defProp.intValue;
#endif
                // Skip if all are default(T) == 0
                if (min == 0 && max == 0 && def == 0)
                    return;

                bool changed = false;

                if (max < min)
                {
                    (min, max) = (max, min);
                    changed = true;
                }
                if (max == min)
                {
                    max = min + 1; // make strictly greater
                    changed = true;
                }

                // Clamp default
                long clampedDef = def < min ? min : (def > max ? max : def);
                if (clampedDef != def)
                {
                    def = clampedDef;
                    changed = true;
                }

                if (changed)
                {
#if UNITY_2021_2_OR_NEWER || UNITY_6000_0_OR_NEWER
                    minProp.longValue = min;
                    maxProp.longValue = max;
                    defProp.longValue = def;
#else
                    minProp.intValue = (int)min;
                    maxProp.intValue = (int)max;
                    defProp.intValue = (int)def;
#endif
                    boardProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                // Not a directly supported numeric type; try reflection path for comparable T
                try_reflection_constraints(boardProperty, minProp, maxProp, defProp);
            }
        }

        private static void try_reflection_constraints(SerializedProperty boardProperty, SerializedProperty minProp,
            SerializedProperty maxProp, SerializedProperty defProp)
        {
#if UNITY_2020_1_OR_NEWER
            // For managedReference, get the runtime instance
            object instance = boardProperty.managedReferenceValue;
            if (instance == null)
                return;

            Type t = instance.GetType();
            // Walk inheritance to find WhiteBoard<T>
            Type whiteboardGeneric = null;
            var cur = t;
            while (cur != null)
            {
                if (cur.IsGenericType && cur.GetGenericTypeDefinition().Name.StartsWith("WhiteBoard"))
                {
                    whiteboardGeneric = cur;
                    break;
                }
                cur = cur.BaseType;
            }
            if (whiteboardGeneric == null)
                return;

            Type valueType = whiteboardGeneric.GetGenericArguments()[0];
            // Only proceed for IComparable
            if (!typeof(IComparable).IsAssignableFrom(valueType))
                return;

            var minPi = t.GetProperty("MinValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var maxPi = t.GetProperty("MaxValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var defPi = t.GetProperty("DefaultValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (minPi == null || maxPi == null || defPi == null)
                return;

            object min = minPi.GetValue(instance);
            object max = maxPi.GetValue(instance);
            object def = defPi.GetValue(instance);

            object defaultT = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
            // Skip if all defaults
            if (Equals(min, defaultT) && Equals(max, defaultT) && Equals(def, defaultT))
                return;

            bool changed = false;
            var cmp = (IComparable)max;
            if (cmp.CompareTo(min) < 0)
            {
                // swap
                (min, max) = (max, min);
                changed = true;
            }
            else if (cmp.CompareTo(min) == 0)
            {
                // make max greater than min by minimal step if numeric; otherwise leave
                if (is_numeric(valueType, out var kind))
                {
                    object step = numeric_step(min, kind);
                    max = numeric_add(min, step, kind);
                    changed = true;
                }
            }

            // Clamp default into [min, max]
            if (is_numeric(valueType, out var nKind))
            {
                var defCmpMin = ((IComparable)def).CompareTo(min);
                var defCmpMax = ((IComparable)def).CompareTo(max);
                if (defCmpMin < 0)
                {
                    def = min;
                    changed = true;
                }
                else if (defCmpMax > 0)
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
                boardProperty.serializedObject.ApplyModifiedProperties();
                var targetObj = boardProperty.serializedObject.targetObject as UnityEngine.Object;
                if (targetObj != null)
                {
                    EditorUtility.SetDirty(targetObj);
                }
            }
#endif
        }

        private enum NumericKind { FloatLike, IntLike }

        private static bool is_numeric(Type t, out NumericKind kind)
        {
            if (t == typeof(float) || t == typeof(double))
            {
                kind = NumericKind.FloatLike;
                return true;
            }
            if (t == typeof(int) || t == typeof(long) || t == typeof(short) ||
                t == typeof(uint) || t == typeof(ulong) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte))
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
                double step = Math.Max(Math.Abs(v) * 1e-6, 1e-6);
                return step;
            }
            else
            {
                return 1L;
            }
        }

        private static object numeric_add(object a, object b, NumericKind kind)
        {
            if (kind == NumericKind.FloatLike)
            {
                double x = Convert.ToDouble(a);
                double y = Convert.ToDouble(b);
                return x + y;
            }
            else
            {
                long x = Convert.ToInt64(a);
                long y = Convert.ToInt64(b);
                return x + y;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float _height = 0f;

            // One line for foldout
            _height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return _height;

            // Check if this is a child stat
            bool _isChildStat = is_child_of_another_stat(property);

            // Add help box height for child stats
            if (_isChildStat)
            {
                _height += EditorGUIUtility.singleLineHeight * 1.5f + EditorGUIUtility.standardVerticalSpacing;
            }

            // Sum heights of direct children except 'Children'
            var _iterator = property.Copy();
            var _end = _iterator.GetEndProperty();
            bool _hasChild = _iterator.NextVisible(true);
            while (_hasChild && !SerializedProperty.EqualContents(_iterator, _end))
            {
                if (_iterator.depth == property.depth + 1 && _iterator.name != "Children")
                {
                    _height += EditorGUI.GetPropertyHeight(_iterator, includeChildren: true) +
                               EditorGUIUtility.standardVerticalSpacing;
                }

                _hasChild = _iterator.NextVisible(false);
            }

            // Add additional child properties height
            if (_isChildStat)
            {
                _height += get_additional_child_properties_height(property);
            }

            // Add Children list height
            var _childrenProp = property.FindPropertyRelative("Children");
            _height += get_children_list_height(property, _childrenProp) + EditorGUIUtility.standardVerticalSpacing;

            return _height;
        }

        /// <summary>
        /// Calculate the height needed for additional child properties
        /// </summary>
        private float get_additional_child_properties_height(SerializedProperty property)
        {
            float _height = 0f;

            // Height for the label
            _height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Add height for any additional properties you draw

            return _height;
        }

        private float draw_children_list(Rect position, SerializedProperty ownerProperty,
            SerializedProperty childrenProp)
        {
            // Prepare (or show help if missing)
            if (childrenProp == null)
            {
                float _h = EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, _h), "Children list not found.",
                    MessageType.Info);
                return _h;
            }

            var _list = get_or_create_list(ownerProperty, childrenProp);

            float _height = _list.GetHeight();
            // Draw
            _list.DoList(new Rect(position.x, position.y, position.width, _height));
            return _height;
        }

        private float get_children_list_height(SerializedProperty ownerProperty, SerializedProperty childrenProp)
        {
            if (childrenProp == null)
                return EditorGUIUtility.singleLineHeight * 2f;

            var _list = get_or_create_list(ownerProperty, childrenProp);
            return _list.GetHeight();
        }

        private ReorderableList get_or_create_list(SerializedProperty ownerProperty, SerializedProperty childrenProp)
        {
            string _key = ownerProperty.propertyPath + ".Children";
            if (lists.TryGetValue(_key, out var _existing))
                return _existing;

            // Ensure list is an array/list
            if (!childrenProp.isArray)
            {
                // If it's null or not initialized, try to draw an empty list safely
                // We still create a list object bound to the property for consistent UI
            }

            var _list = new ReorderableList(childrenProp.serializedObject, childrenProp, true, true, true, true);

            _list.drawHeaderCallback = rect =>
            {
                int _count = childrenProp.isArray ? childrenProp.arraySize : 0;
                // Label
                EditorGUI.LabelField(rect, $"Children ({_count})");

                // Clear button aligned to the right
                var _buttonWidth = 60f;
                var _buttonRect = new Rect(rect.xMax - _buttonWidth, rect.y, _buttonWidth, rect.height);
                using (new EditorGUI.DisabledScope(_count == 0))
                {
                    if (GUI.Button(_buttonRect, "Clear"))
                    {
                        Undo.RecordObject(childrenProp.serializedObject.targetObject, "Clear Children");
                        childrenProp.ClearArray();
                        childrenProp.serializedObject.ApplyModifiedProperties();
                        GUI.FocusControl(null);
                    }
                }
            };

            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var _element = childrenProp.GetArrayElementAtIndex(index);
                StatListGUI.DrawSerializableStatElement(rect, _element, index, isActive, isFocused);
            };

            _list.elementHeightCallback = index =>
            {
                var _element = childrenProp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(_element, includeChildren: true) + 4f;
            };

            lists[_key] = _list;
            return _list;
        }
    }
}