#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using DAFP.TOOLS.ECS.Serialization;
using PixelRouge.Inspector.Extensions;
using PixelRouge.Inspector.Utilities;
using PixelRouge.Inspector.CustomStructures;

namespace PixelRouge.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class ImprovedEditor : Editor
    {
        #region Fields

        #region Attribute info storing

        private IEnumerable<SerializedProperty> _groupFields;
        private IEnumerable<IGrouping<string, SerializedProperty>> _boxGroupedFields;
        private IEnumerable<IGrouping<string, SerializedProperty>> _foldoutGroupedFields;
        private IEnumerable<SerializedProperty> _foldoutGroupFields;
        private List<SerializedProperty> _reorderableListProperties;
        private IEnumerable<SerializedProperty> _ungroupedFields;
        private IEnumerable<FieldInfo> _nonSerializedFields;
        private IEnumerable<PropertyInfo> _nativeProperties;
        private IEnumerable<MethodInfo> _methods;
        private IEnumerable<MethodInfo> _methodsNoArguments;
        private IEnumerable<MethodInfo> _methodsWithArguments;
        private List<SerializedProperty> _serializedProperties = new List<SerializedProperty>();
        private Dictionary<string, SavedBool> _foldoutStates = new Dictionary<string, SavedBool>();
        private HashSet<ReorderableList> _reorderableLists = new HashSet<ReorderableList>();
        public bool DisableEverything;

        #endregion

        #region Settings

        private bool _drawScriptField = true;
        private bool _drawUngroupedFieldsFirst = false;
        private bool _foldNonSerializedFields = true;

        #endregion

        #region Control fields

        private bool _showFoldedNonSerializedFields = true;

        #endregion

        #region Constant fields

        private const string NonSerializedFieldsHeader = "Non-serialized fields";

        #endregion

        #endregion

        #region Unity events

        private void OnEnable()
        {
            _serializedProperties = new List<SerializedProperty>();
            _serializedProperties = FindSerializedProperties(_serializedProperties);
            FindAttributes();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _serializedProperties = FindSerializedProperties(_serializedProperties);
            DrawFields();
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Drawing

        private void DrawScriptField()
        {
            if (!_drawScriptField)
                return;

            foreach (var field in _ungroupedFields)
            {
                if (field.name.Equals("m_Script", StringComparison.Ordinal))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(field);
                    EditorGUI.EndDisabledGroup();
                    break;
                }
            }
        }

        private void DrawFields()
        {
            DrawScriptField();
            DrawFieldsBySections();
        }

        private void DrawFieldsBySections()
        {
            if (_drawUngroupedFieldsFirst)
                DrawUngroupedFields();

            DrawReoderableLists();
            DrawAllGroups();
            DrawAllFoldoutGroups();

            if (!_drawUngroupedFieldsFirst)
                DrawUngroupedFields();

            DrawNonSerializedProperties();
            DrawButtons();
        }

        protected void DrawButtons()
        {
            if (!_methods.Any())
                return;

            EditorGUILayout.Space();
            foreach (var method in _methods)
            {
                var buttonAttribute = Attribute.GetCustomAttribute(method, typeof(ButtonAttribute)) as ButtonAttribute;
                if (buttonAttribute != null)
                    DrawButton(buttonAttribute, method);
            }
        }

        private void DrawButton(ButtonAttribute buttonAttribute, MethodInfo method)
        {
            if (!EditorUtil.IsButtonVisible(serializedObject.targetObject, method))
                return;

            var backupBgColor = GUI.backgroundColor;
            if (buttonAttribute.Color != Color.clear)
                GUI.backgroundColor = buttonAttribute.Color;

            var prevGui = GUI.enabled;
            GUI.enabled = HandleButtonMode(buttonAttribute.ButtonMode);

            if (GUI.enabled && !EditorUtil.IsButtonEnabled(serializedObject.targetObject, method))
                GUI.enabled = false;
            else if (!GUI.enabled && EditorUtil.IsButtonEnabled(serializedObject.targetObject, method))
                GUI.enabled = true;

            var buttonName = string.IsNullOrEmpty(buttonAttribute.Label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : buttonAttribute.Label;

            if (GUILayout.Button(buttonName))
            {
                var args = buttonAttribute.Arguments ?? method.GetParameters().Select(p => p.DefaultValue).ToArray();
                method.Invoke(serializedObject.targetObject, args);
            }

            GUI.enabled = prevGui;
            GUI.backgroundColor = backupBgColor;
        }

        private bool HandleButtonMode(EButtonMode buttonMode)
        {
            switch (buttonMode)
            {
                case EButtonMode.AlwaysEnabled: return true;
                case EButtonMode.EditorOnly:   return !EditorApplication.isPlaying;
                case EButtonMode.PlayModeOnly: return EditorApplication.isPlaying;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawGroup(IGrouping<string, SerializedProperty> group)
        {
            BeginGroup(group.Key);
            foreach (var field in group)
                if (field.IsVisible())
                    using (new EditorGUILayout.VerticalScope(EditorUtil.GroupBackgroundStyle()))
                        DrawSerializedProperty(field, true);
            EndGroup();
        }

        private void DrawAllGroups()
        {
            foreach (var group in _boxGroupedFields)
            {
                DrawGroup(group);
                EditorGUILayout.Space();
            }
        }

        private void DrawFoldoutGroup(IGrouping<string, SerializedProperty> foldoutGroup)
        {
            var key = foldoutGroup.Key;
            if (!_foldoutStates.ContainsKey(key))
                _foldoutStates[key] = new SavedBool($"{target.GetInstanceID()}.{key}", false);

            _foldoutStates[key].Value =
                EditorGUILayout.Foldout(_foldoutStates[key].Value, key, EditorUtil.FoldoutStyle());

            if (!_foldoutStates[key].Value)
                return;

            foreach (var field in foldoutGroup)
                if (field.IsVisible())
                    using (new EditorGUI.IndentLevelScope(EditorUtil.FoldoutIndent))
                        DrawSerializedProperty(field, true);
        }

        private void DrawAllFoldoutGroups()
        {
            foreach (var foldoutGroup in _foldoutGroupedFields)
            {
                GUI.enabled = !DisableEverything;
                DrawFoldoutGroup(foldoutGroup);
                GUI.enabled = !DisableEverything;
            }
        }

        private void DrawReoderableLists()
        {
            foreach (var list in _reorderableLists)
            {
                GUI.enabled = !DisableEverything;
                list.DoLayoutList();
                GUI.enabled = !DisableEverything;
            }
        }

        private void DrawUngroupedFields()
        {
            bool skippedScript = false;
            foreach (var propertyField in _ungroupedFields)
            {
                // Skip the script field (we already drew it)
                if (!skippedScript && propertyField.name == "m_Script")
                {
                    skippedScript = true;
                    continue;
                }

                // determine enable state
                bool prev = GUI.enabled;
                if (propertyField.GetAttribute<NeverReadOnly>() != null)
                    GUI.enabled = true;
                else
                    GUI.enabled = !DisableEverything;

                DrawSerializedProperty(propertyField);
                GUI.enabled = prev;
            }
        }

        private void DrawNonSerializedProperties()
        {
            GUI.enabled = !DisableEverything;
            if (!_nonSerializedFields.Any() && !_nativeProperties.Any())
                return;

            if (_foldNonSerializedFields)
            {
                EditorGUILayout.Space();
                _showFoldedNonSerializedFields = EditorGUILayout.Foldout(
                    _showFoldedNonSerializedFields, NonSerializedFieldsHeader, EditorUtil.FoldoutStyle());
            }

            if (_foldNonSerializedFields && !_showFoldedNonSerializedFields)
                return;

            foreach (var field in _nonSerializedFields)
            {
                GUI.enabled = !DisableEverything;
                if (_foldNonSerializedFields)
                    using (new EditorGUI.IndentLevelScope(EditorUtil.FoldoutIndent))
                        DrawNonSerializedProperty(serializedObject.targetObject, field);
                else
                    DrawNonSerializedProperty(serializedObject.targetObject, field);

                GUI.enabled = !DisableEverything;
            }

            foreach (var prop in _nativeProperties)
            {
                GUI.enabled = !DisableEverything;
                if (_foldNonSerializedFields)
                    using (new EditorGUI.IndentLevelScope(EditorUtil.FoldoutIndent))
                        DrawNonSerializedProperty(serializedObject.targetObject, prop);
                else
                    DrawNonSerializedProperty(serializedObject.targetObject, prop);

                GUI.enabled = !DisableEverything;
            }
        }

        private void DrawSeparatorLine(Color color)
        {
            var style = EditorUtil.HorizontalLineStyle();
            var old = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, style);
            GUI.color = old;
        }

        private void DrawListHeader(Rect rect)
        {
            var list = GetCurrentReordableList();
            GUI.Label(rect, ObjectNames.NicifyVariableName(list.name));
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            serializedObject.Update();
            var list = GetCurrentReordableList();
            var item = list.GetArrayElementAtIndex(index);

            bool prev = GUI.enabled;
            if (item.GetAttribute<NeverReadOnly>() != null)
                GUI.enabled = true;
            else
                GUI.enabled = !DisableEverything;

            EditorGUI.PropertyField(rect, item);
            GUI.enabled = prev;

            serializedObject.ApplyModifiedProperties();
        }

        // Overridden to respect [NeverReadOnly]
        private void DrawSerializedProperty(SerializedProperty property)
        {
            bool prev = GUI.enabled;
            if (property.GetAttribute<NeverReadOnly>() != null)
                GUI.enabled = true;
            // else GUI.enabled remains as it was (set by caller)
            EditorGUILayout.PropertyField(property);
            GUI.enabled = prev;
        }

        private void DrawSerializedProperty(SerializedProperty property, bool includeChildren)
        {
            bool prev = GUI.enabled;
            if (property.GetAttribute<NeverReadOnly>() != null)
                GUI.enabled = true;
            EditorGUILayout.PropertyField(property, includeChildren);
            GUI.enabled = prev;
        }

        private void DrawNonSerializedProperty(UnityEngine.Object targetObject, FieldInfo fieldInfo)
        {
            DrawDisabledProperty(fieldInfo.GetValue(targetObject), ObjectNames.NicifyVariableName(fieldInfo.Name));
        }

        private void DrawNonSerializedProperty(UnityEngine.Object targetObject, PropertyInfo propertyInfo)
        {
            DrawDisabledProperty(propertyInfo.GetValue(targetObject),
                ObjectNames.NicifyVariableName(propertyInfo.Name));
        }

        private void DrawDisabledProperty(object value, string labelText)
        {
            if (value == null)
            {
                DrawErrorMessage($"Value {value} is null, can't draw field.");
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                var vt = value.GetType();
                if (vt == typeof(bool))       EditorGUILayout.ToggleLeft(labelText, (bool)value);
                else if (vt == typeof(int))   EditorGUILayout.IntField(labelText, (int)value);
                else if (vt == typeof(long))  EditorGUILayout.LongField(labelText, (long)value);
                else if (vt == typeof(float)) EditorGUILayout.FloatField(labelText, (float)value);
                else if (vt == typeof(double))EditorGUILayout.DoubleField(labelText, (double)value);
                else if (vt == typeof(string))EditorGUILayout.TextField(labelText, (string)value);
                else if (vt == typeof(Vector2))EditorGUILayout.Vector2Field(labelText, (Vector2)value);
                else if (vt == typeof(Vector3))EditorGUILayout.Vector3Field(labelText, (Vector3)value);
                else if (vt == typeof(Vector4))EditorGUILayout.Vector4Field(labelText, (Vector4)value);
                else if (vt == typeof(Color))  EditorGUILayout.ColorField(labelText, (Color)value);
                else if (vt == typeof(Bounds)) EditorGUILayout.BoundsField(labelText, (Bounds)value);
                else if (vt == typeof(Rect))   EditorGUILayout.RectField(labelText, (Rect)value);
                else if (typeof(UnityEngine.Object).IsAssignableFrom(vt))
                    EditorGUILayout.ObjectField(labelText, (UnityEngine.Object)value, vt, true);
                else if (vt.IsEnum)             EditorGUILayout.EnumPopup(labelText, (Enum)value);
                else if (vt.BaseType == typeof(TypeInfo))
                    EditorGUILayout.TextField(labelText, value.ToString());
                else
                    DrawErrorMessage($"Can't draw a field of type {vt}.");
            }
        }

        private void DrawErrorMessage(string message)
        {
            var col = GUI.contentColor;
            GUI.contentColor = Color.red;
            EditorGUILayout.LabelField(message);
            GUI.contentColor = col;
        }

        #endregion

        #region Property info collection

        private List<SerializedProperty> FindSerializedProperties(List<SerializedProperty> properties)
        {
            properties.Clear();
            try
            {
                using (var iterator = serializedObject.GetIterator())
                {
                    if (iterator.NextVisible(true))
                    {
                        do
                        {
                            properties.Add(serializedObject.FindProperty(iterator.name));
                        } while (iterator.NextVisible(false));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return properties;
        }

        private void FindAttributes()
        {
            CollectUngroupedSerializedFieldsInfo();
            CollectNonSerializedFieldInfo();
            CollectMethodsInfo();
            CollectGroupInfo();
        }

        private void CollectUngroupedSerializedFieldsInfo()
        {
            _ungroupedFields = _serializedProperties
                .Where(p => p.GetAttribute<GroupAttribute>() == null)
                .Where(p => p.GetAttribute<FoldoutAttribute>() == null);
        }

        private void CollectNonSerializedFieldInfo()
        {
            _nonSerializedFields = target.GetAllFields(f =>
                f.GetCustomAttributes(typeof(ShowNonSerializedFieldAttribute), true).Length > 0);

            _nativeProperties =
                target.GetAllProperties(p => p.GetCustomAttributes(typeof(ShowPropertyAttribute), true).Length > 0);
        }

        private void CollectMethodsInfo()
        {
            _methods = target.GetAllMethods(m =>
                m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

            _methodsNoArguments = _methods.Where(m => m.GetParameters().Length == 0);
            _methodsWithArguments = _methods.Where(m => m.GetParameters().Length > 0);
        }

        private void CollectGroupInfo()
        {
            _groupFields = _serializedProperties
                .Where(p => p.GetAttribute<GroupAttribute>() != null);

            _boxGroupedFields = _groupFields.GroupBy(f => f.GetAttribute<GroupAttribute>().Name);

            _foldoutGroupFields = _serializedProperties
                .Where(p => p.GetAttribute<FoldoutAttribute>() != null);

            _foldoutGroupedFields = _foldoutGroupFields.GroupBy(f => f.GetAttribute<FoldoutAttribute>().Name);
        }

        #endregion

        #region Grouping auxiliar methods

        private IGrouping<string, SerializedProperty> GetBoxGroupWithName(string name)
        {
            return _boxGroupedFields.SingleOrDefault(g => g.Key == name);
        }

        private IGrouping<string, SerializedProperty> GetFoldoutGroupWithName(string name)
        {
            return _foldoutGroupedFields.SingleOrDefault(g => g.Key == name);
        }

        private void BeginGroup(string groupName)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (!string.IsNullOrEmpty(groupName))
                EditorGUILayout.LabelField(groupName, EditorUtil.GroupLabelStyle());
        }

        private void EndGroup()
        {
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Reorderable list auxiliar methods

        private SerializedProperty GetCurrentReordableList()
        {
            var lists = _reorderableListProperties.ToArray();
            if (_reorderableListProperties.Count <= 1)
                return lists[0];
            // only one supported
            return lists[0];
        }

        #endregion
    }
}
#endif