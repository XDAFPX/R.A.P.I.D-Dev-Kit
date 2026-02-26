using RapidLib.DAFP.TOOLS.Common;

namespace RapidLib.DAFP.TOOLS.Editor
{
    using UnityEditor;
    using UnityEngine;


    [CustomPropertyDrawer(typeof(PriorityAttribute))]
    public class PriorityDrawer : PropertyDrawer
    {
        private const float NumberFieldWidth = 40f;
        private const float ArrowWidth = 16f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use [Priority] with int only.");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float labelWidth = EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            float fieldX = position.x + labelWidth;

            Rect fieldRect = new Rect(fieldX, position.y, NumberFieldWidth, position.height);
            property.intValue = EditorGUI.IntField(fieldRect, property.intValue);

            float arrowX = fieldX + NumberFieldWidth + 1f;
            float arrowH = Mathf.Floor(position.height / 2f);

            GUIStyle arrowStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0,0,0,0),
                
                fontSize = 9,
            };

            Rect upRect = new Rect(arrowX, position.y, ArrowWidth*2, arrowH);
            Rect downRect = new Rect(arrowX, position.y + arrowH, ArrowWidth*2, arrowH);

            if (GUI.Button(upRect, "▲", arrowStyle)) property.intValue++;
            if (GUI.Button(downRect, "▼", arrowStyle)) property.intValue--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight+15;
        }
    }
}