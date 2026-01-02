using System.Collections.Generic;
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
        private readonly Dictionary<string, ReorderableList> _lists = new Dictionary<string, ReorderableList>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin property scope
            EditorGUI.BeginProperty(position, label, property);

            // Check if this WhiteBoard is a child of another stat
            bool isChildStat = IsChildOfAnotherStat(property);
            
            // Calculate rects for fields
            float y = position.y;
            float x = position.x;
            float width = position.width;

            // Draw foldout for the root (managed reference types often show a foldout)
            property.isExpanded = EditorGUI.Foldout(new Rect(x, y, width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Show info box if this is a child stat
                if (isChildStat)
                {
                    float _separatorH = EditorGUIUtility.singleLineHeight * 0.1f;
                    EditorGUI.DrawRect(new Rect(x,y,width,_separatorH), Color.crimson);
                    y += _separatorH + EditorGUIUtility.standardVerticalSpacing;
                }

                // Iterate direct children excluding 'Children'
                var iterator = property.Copy();
                var end = iterator.GetEndProperty();

                // Move to first visible child
                bool hasChild = iterator.NextVisible(true);
                while (hasChild && !SerializedProperty.EqualContents(iterator, end))
                {
                    // Only draw direct children of this property
                    if (iterator.depth == property.depth + 1 && iterator.name != "Children")
                    {
                        float h = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                        EditorGUI.PropertyField(new Rect(x, y, width, h), iterator, true);
                        y += h + EditorGUIUtility.standardVerticalSpacing;
                    }

                    hasChild = iterator.NextVisible(false);
                }

                // Draw additional properties for child stats
                if (isChildStat)
                {
                    DrawAdditionalChildProperties(ref y, x, width, property);
                }

                // Draw Children list at the end
                var childrenProp = property.FindPropertyRelative("Children");
                float listHeight = DrawChildrenList(new Rect(x, y, width, 0), property, childrenProp);
                y += listHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Checks if this property is inside a "Children" array of another stat
        /// </summary>
        private bool IsChildOfAnotherStat(SerializedProperty property)
        {
            // Check if the property path contains ".Children.Array.data["
            // This indicates it's an element within a Children list
            return property.propertyPath.Contains(".Children.Array.data[");
        }

        /// <summary>
        /// Draw additional properties that should only appear for child stats
        /// </summary>
        private void DrawAdditionalChildProperties(ref float y, float x, float width, SerializedProperty property)
        {
            
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0f;

            // One line for foldout
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return height;

            // Check if this is a child stat
            bool isChildStat = IsChildOfAnotherStat(property);

            // Add help box height for child stats
            if (isChildStat)
            {
                height += EditorGUIUtility.singleLineHeight * 1.5f + EditorGUIUtility.standardVerticalSpacing;
            }

            // Sum heights of direct children except 'Children'
            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            bool hasChild = iterator.NextVisible(true);
            while (hasChild && !SerializedProperty.EqualContents(iterator, end))
            {
                if (iterator.depth == property.depth + 1 && iterator.name != "Children")
                {
                    height += EditorGUI.GetPropertyHeight(iterator, includeChildren: true) +
                              EditorGUIUtility.standardVerticalSpacing;
                }

                hasChild = iterator.NextVisible(false);
            }

            // Add additional child properties height
            if (isChildStat)
            {
                height += GetAdditionalChildPropertiesHeight(property);
            }

            // Add Children list height
            var childrenProp = property.FindPropertyRelative("Children");
            height += GetChildrenListHeight(property, childrenProp) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        /// <summary>
        /// Calculate the height needed for additional child properties
        /// </summary>
        private float GetAdditionalChildPropertiesHeight(SerializedProperty property)
        {
            float height = 0f;

            // Height for the label
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Add height for any additional properties you draw

            return height;
        }

        private float DrawChildrenList(Rect position, SerializedProperty ownerProperty, SerializedProperty childrenProp)
        {
            // Prepare (or show help if missing)
            if (childrenProp == null)
            {
                float h = EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, h), "Children list not found.",
                    MessageType.Info);
                return h;
            }

            var list = GetOrCreateList(ownerProperty, childrenProp);

            float height = list.GetHeight();
            // Draw
            list.DoList(new Rect(position.x, position.y, position.width, height));
            return height;
        }

        private float GetChildrenListHeight(SerializedProperty ownerProperty, SerializedProperty childrenProp)
        {
            if (childrenProp == null)
                return EditorGUIUtility.singleLineHeight * 2f;

            var list = GetOrCreateList(ownerProperty, childrenProp);
            return list.GetHeight();
        }

        private ReorderableList GetOrCreateList(SerializedProperty ownerProperty, SerializedProperty childrenProp)
        {
            string key = ownerProperty.propertyPath + ".Children";
            if (_lists.TryGetValue(key, out var existing))
                return existing;

            // Ensure list is an array/list
            if (!childrenProp.isArray)
            {
                // If it's null or not initialized, try to draw an empty list safely
                // We still create a list object bound to the property for consistent UI
            }

            var list = new ReorderableList(childrenProp.serializedObject, childrenProp, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                int count = childrenProp.isArray ? childrenProp.arraySize : 0;
                // Label
                EditorGUI.LabelField(rect, $"Children ({count})");

                // Clear button aligned to the right
                var buttonWidth = 60f;
                var buttonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);
                using (new EditorGUI.DisabledScope(count == 0))
                {
                    if (GUI.Button(buttonRect, "Clear"))
                    {
                        Undo.RecordObject(childrenProp.serializedObject.targetObject, "Clear Children");
                        childrenProp.ClearArray();
                        childrenProp.serializedObject.ApplyModifiedProperties();
                        GUI.FocusControl(null);
                    }
                }
            };

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = childrenProp.GetArrayElementAtIndex(index);
                StatListGUI.DrawSerializableStatElement(rect, element, index, isActive, isFocused);
            };

            list.elementHeightCallback = index =>
            {
                var element = childrenProp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, includeChildren: true) + 4f;
            };

            _lists[key] = list;
            return list;
        }
    }
}