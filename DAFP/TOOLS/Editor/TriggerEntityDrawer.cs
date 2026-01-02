using DAFP.TOOLS.ECS.Environment;
using PixelRouge.Inspector;
using TNRD;

namespace RapidLib.DAFP.TOOLS.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;

    [CustomEditor(typeof(TriggerEntity))]
    public class TriggerEntityEditor : ImprovedEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    switch (prop.name)
                    {
                        case "Filters":
                            DrawColoredFiltersList(prop);
                            break;
                        case "Actions":
                            DrawColoredActionsList(prop);
                            break;
                        case "_brains":
                        case "Tag":
                        case "Variety":
                            break;
                        default:
                            EditorGUILayout.PropertyField(prop, true);
                            break;
                    }
                } while (prop.NextVisible(false));
            }

            DrawButtons();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColoredActionsList(SerializedProperty listProperty)
        {
            EditorGUILayout.PropertyField(listProperty, new GUIContent("Actions"), false);

            if (listProperty.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Size field with +/- buttons
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(listProperty.FindPropertyRelative("Array.size"));
                EditorGUILayout.EndHorizontal();

                var targetObject = serializedObject.targetObject;
                var field = targetObject.GetType().GetField("Actions",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                var filtersList = field?.GetValue(targetObject) as List<SerializableInterface<ITriggerAction>>;

                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);

                    // Determine color
                    Color boxColor = Color.gray;
                    if (filtersList != null && i < filtersList.Count)
                    {
                        var filter = filtersList[i]?.Value;
                        if (filter != null)
                        {
                            boxColor = new Color(1f, 0.5f, 0, 1);
                        }
                    }

                    // Draw colored box with element and buttons
                    var originalBgColor = GUI.backgroundColor;
                    GUI.backgroundColor = boxColor;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUI.backgroundColor = originalBgColor;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element, new GUIContent($"Element {i}"), true);

                    // Add duplicate button
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        listProperty.InsertArrayElementAtIndex(i);
                    }

                    // Remove button
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                // Add button at the bottom
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Element", GUILayout.Width(100)))
                {
                    listProperty.arraySize++;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        private void DrawColoredFiltersList(SerializedProperty listProperty)
        {
            EditorGUILayout.PropertyField(listProperty, new GUIContent("Filters"), false);

            if (listProperty.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Size field with +/- buttons
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(listProperty.FindPropertyRelative("Array.size"));
                EditorGUILayout.EndHorizontal();

                var targetObject = serializedObject.targetObject;
                var field = targetObject.GetType().GetField("Filters",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                var filtersList = field?.GetValue(targetObject) as List<SerializableInterface<ITriggerFilter>>;

                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);

                    // Determine color
                    Color boxColor = Color.gray;
                    if (filtersList != null && i < filtersList.Count)
                    {
                        var filter = filtersList[i]?.Value;
                        if (filter != null)
                        {
                            boxColor = filter.LastStatus switch
                            {
                                true => new Color(0.3f, 0.8f, 0.3f),
                                false => new Color(0.8f, 0.3f, 0.3f),
                                null => new Color(0.8f, 0.8f, 0.3f)
                            };
                        }
                    }

                    // Draw colored box with element and buttons
                    var originalBgColor = GUI.backgroundColor;
                    GUI.backgroundColor = boxColor;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUI.backgroundColor = originalBgColor;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element, new GUIContent($"Element {i}"), true);

                    // Add duplicate button
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        listProperty.InsertArrayElementAtIndex(i);
                    }

                    // Remove button
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                // Add button at the bottom
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Element", GUILayout.Width(100)))
                {
                    listProperty.arraySize++;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif