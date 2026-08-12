using RapidLib.DAFP.TOOLS.Common;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Common.Editor
{
    [CustomPropertyDrawer(typeof(AddressableEntry))]
    public class AddressableEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var assetRefProp = property.FindPropertyRelative("assetRef");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect refRect = new Rect(position.x, position.y, position.width, lineHeight);
            Rect addressRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(refRect, assetRefProp, label);
            bool changed = EditorGUI.EndChangeCheck();

            // Resolve the target AddressableEntry instance directly.
            AddressableEntry entry = property.boxedValue as AddressableEntry;

            if (changed)
            {
                property.serializedObject.ApplyModifiedProperties();
                RefreshAddress(entry, property.serializedObject.targetObject);
                
                property.boxedValue = entry;
                property.serializedObject.ApplyModifiedProperties();
            }

            string displayValue = (entry != null && !string.IsNullOrEmpty(entry.Address))
                ? entry.Address
                : "<not addressable / no entry found>";

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.TextField(addressRect, "Resolved Address", displayValue);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            return lineHeight * 2 + spacing;
        }

        private static void RefreshAddress(AddressableEntry entry, Object owningObject)
        {
            if (entry == null || entry.assetRef == null || string.IsNullOrEmpty(entry.assetRef.AssetGUID))
            {
                if (entry != null) entry.Address = null;
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                entry.Address = null;
                return;
            }

            var assetEntry = settings.FindAssetEntry(entry.assetRef.AssetGUID);
            entry.Address = assetEntry != null ? assetEntry.address : null;

            if (owningObject != null)
                EditorUtility.SetDirty(owningObject);
        }

        // --- Reflection helper: walks a SerializedProperty path back to the
        // actual managed object instance (handles nested fields and arrays). ---
        private static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = System.Convert.ToInt32(
                        element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            var field = type.GetField(name,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
            if (field != null) return field.GetValue(source);

            var property = type.GetProperty(name,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
            return property != null ? property.GetValue(source, null) : null;
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;

            var enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext()) return null;
            }

            return enumerator.Current;
        }
    }
}