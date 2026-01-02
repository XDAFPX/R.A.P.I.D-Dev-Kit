using System;
using System.Linq;
using DAFP.TOOLS.ECS.BuiltIn;
using UnityEditor;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [System.Serializable]
    public class TagFilter : ITriggerFilter, IFilter<IEntity>
    {
        [HideInInspector] public int TagMask;

        public bool Evaluate(IEntity go)
        {
            return Evaluate(go.GetWorldRepresentation());
        }

        public bool Evaluate(GameObject go)
        {
            if (go == null) return false;
            var _allTags = TagRegistry.AllTags;
            if (TagMask == 0 || _allTags == null) return false;
            bool _tagMatched = _allTags.Where((_t, _i) => ((1 << _i) & TagMask) != 0 && go.CompareTag(_t)).Any();

            return _tagMatched;
        }

        public TriggerEntity.TriggerEvent Event { get; set; }
        public bool? LastStatus { get; set; }
    }
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(TagFilter))]
    public class TagFilterDrawer : PropertyDrawer
    {
        private static string[] _cachedTags;

        static TagFilterDrawer()
        {
            _cachedTags = UnityEditorInternal.InternalEditorUtility.tags;
        }

        private static bool _foldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _foldout = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                _foldout,
                label,
                true
            );

            if (_foldout)
            {
                EditorGUI.indentLevel++;
                float _lineHeight = EditorGUIUtility.singleLineHeight + 2;

                var _tagMaskProp = property.FindPropertyRelative("TagMask");


                // Маска тегів
                _tagMaskProp.intValue = EditorGUI.MaskField(
                    new Rect(position.x, position.y + _lineHeight, position.width,
                        EditorGUIUtility.singleLineHeight),
                    "Tags",
                    _tagMaskProp.intValue,
                    _cachedTags
                );

                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float _lineHeight = EditorGUIUtility.singleLineHeight * 2;
            return _foldout ? _lineHeight : EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}