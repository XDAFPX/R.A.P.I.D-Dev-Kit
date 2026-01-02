using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Environment.Filters;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    using System;
    using UnityEngine;

    [Serializable]
    public class EntityTypeFilter : IFilter<GameObject>, IFilter<IEntity>, ITriggerFilter
    {
        [HideInInspector] public int typeMask; // багатотовий вибір типів IEntity
        private static Type[] _entAsemblies = null;


        public static Type[] ENT_ASSEMBLIES
        {
            get
            {
                if (_entAsemblies == null)
                {
                    _entAsemblies =
                        AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => typeof(IEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                            .ToArray();
                }

                return _entAsemblies;
            }
        }

        public bool Matches(IEntity other, Type[] entityTypes)
        {
            if (other == null) return false;

            // Перевірка типу
            if (typeMask == 0) return true;
            bool matched = false;
            for (int i = 0; i < entityTypes.Length; i++)
            {
                if (((1 << i) & typeMask) != 0 && entityTypes[i].IsAssignableFrom(other.GetType()))
                {
                    matched = true;
                    break;
                }
            }

            if (!matched) return false;


            return true;
        }

        public bool Matches(IEntity other)
        {
            return Matches(other, ENT_ASSEMBLIES);
        }


        public TriggerEntity.TriggerEvent Event { get; set; }

        public bool Evaluate(GameObject go)
        {
            return go.TryGetComponent<IEntity>(out var _entity) && Matches(_entity);
        }

        public bool Evaluate(IEntity go)
        {
            return Matches(go);
        }

        public bool EvalTrigger(TriggerEntity.TriggerEvent @event, TriggerCollider target)
        {
            var val = Evaluate(target.GameObject);
            LastStatus = val;
            return val;
        }


        public bool? LastStatus { get; set; }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(EntityTypeFilter))]
    public class EntityFilterDrawer : PropertyDrawer
    {
        private static Type[] cachedEntityTypes;
        private static string[] cachedEntityTypeNames;

        private static string[] cachedTags;

        static EntityFilterDrawer()
        {
            cachedEntityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();

            cachedEntityTypeNames = cachedEntityTypes.Select(t => t.Name).ToArray();

            // Кешуємо усі теги проекту
            cachedTags = UnityEditorInternal.InternalEditorUtility.tags;
        }

        private static bool foldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            foldout = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                foldout,
                label,
                true
            );

            if (foldout)
            {
                EditorGUI.indentLevel++;
                float lineHeight = EditorGUIUtility.singleLineHeight + 2;

                var typeMaskProp = property.FindPropertyRelative("typeMask");

                // Маска типів
                typeMaskProp.intValue = EditorGUI.MaskField(
                    new Rect(position.x, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight),
                    "Entity Types",
                    typeMaskProp.intValue,
                    cachedEntityTypeNames
                );

                // Інші поля

                EditorGUI.indentLevel--;
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            return foldout ? lineHeight * 2 : lineHeight; // 3 маски + 4 поля + foldout
        }
    }
#endif
}