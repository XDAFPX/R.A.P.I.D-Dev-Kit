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

        private void OnEnable()
        {
            var statsProperty = serializedObject.FindProperty("Stats");

            _statsList = new ReorderableList(serializedObject, statsProperty, true, true, true, true);

            // Draw the header
            _statsList.drawHeaderCallback = (Rect rect) =>
            {
                // Label
                EditorGUI.LabelField(rect, $"Stats ({statsProperty.arraySize})");

                // Clear button aligned to the right
                var buttonWidth = 60f;
                var buttonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);
                using (new EditorGUI.DisabledScope(statsProperty.arraySize == 0))
                {
                    if (GUI.Button(buttonRect, "Clear"))
                    {
                        Undo.RecordObject(serializedObject.targetObject, "Clear Stats");
                        statsProperty.ClearArray();
                        serializedObject.ApplyModifiedProperties();
                        GUI.FocusControl(null);
                    }
                }
            };

            // Draw each element with colored background derived from the stat name
            _statsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = statsProperty.GetArrayElementAtIndex(index);
                StatListGUI.DrawSerializableStatElement(rect, element, index, isActive, isFocused);
            };

            // Element height
            _statsList.elementHeightCallback = (int index) =>
            {
                var element = statsProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element) + 4;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw other properties
            DrawPropertiesExcluding(serializedObject, "Stats");

            EditorGUILayout.Space();

            // Draw the custom list
            _statsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}