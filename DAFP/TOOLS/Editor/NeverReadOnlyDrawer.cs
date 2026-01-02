using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.ECS.Serialization;
using UnityEditor;
using UnityEngine;

namespace Archon.SwissArmyLib.Editor.Utils
{
    /// <summary>
    /// Makes fields marked with <see cref="ReadOnlyAttribute"/> uninteractable via the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(NeverReadOnly))]
    public class NeverReadOnlyDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = true;
            EditorGUI.PropertyField(position, property, label, true);
        }

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Ensure nested/complex properties reserve the correct height
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}