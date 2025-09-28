using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace EnhancedTriggerbox.Component
{
    [AddComponentMenu("")]
    public class EventResponse : ResponseComponent
    {
        [SerializeField] private UnityEvent Event;

#if UNITY_EDITOR
        public override void DrawInspectorGUI()
        {
 // Step 1: Wrap this component in a SerializedObject
            SerializedObject so = new SerializedObject(this);
            // Step 2: Pull current serialized data
            so.Update();

            // Step 3: Find the 'Event' property
            SerializedProperty eventProp = so.FindProperty("Event");
            // Step 4: Draw the UnityEvent with foldout for listeners
            EditorGUILayout.PropertyField(eventProp, new GUIContent("Event"), true);

            // Step 5: Write back any property changes
            so.ApplyModifiedProperties();
        }
#endif
        public override void Validation()
        {
        }

        public override bool ExecuteAction()
        {
            Event.Invoke();
            return true;
        }
    }
}