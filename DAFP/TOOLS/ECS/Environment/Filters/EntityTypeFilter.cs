using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.Environment.TriggerSys;
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
        [HideInInspector] public List<string> selectedTypeNames = new();

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

        public bool Matches(IEntity other)
        {
            if (other == null) return false;
            if (selectedTypeNames == null || selectedTypeNames.Count == 0) return true;

            var type = other.GetType();
            return selectedTypeNames.Any(name =>
                ENT_ASSEMBLIES.FirstOrDefault(t => t.Name == name) is { } t &&
                t.IsAssignableFrom(type));
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

        static EntityFilterDrawer()
        {
            cachedEntityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();

            cachedEntityTypeNames = cachedEntityTypes.Select(t => t.Name).ToArray();
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

            if (!foldout) return;

            EditorGUI.indentLevel++;
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;

            var selectedTypeNamesProp = property.FindPropertyRelative("selectedTypeNames");

            // Build current mask from saved names
            int currentMask = 0;
            for (int i = 0; i < cachedEntityTypeNames.Length; i++)
            {
                for (int j = 0; j < selectedTypeNamesProp.arraySize; j++)
                {
                    if (selectedTypeNamesProp.GetArrayElementAtIndex(j).stringValue == cachedEntityTypeNames[i])
                    {
                        currentMask |= (1 << i);
                        break;
                    }
                }
            }

            // Draw mask field
            int newMask = EditorGUI.MaskField(
                new Rect(position.x, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight),
                "Entity Types",
                currentMask,
                cachedEntityTypeNames
            );

            // Save back as names if changed
            if (newMask != currentMask)
            {
                selectedTypeNamesProp.ClearArray();
                int idx = 0;
                for (int i = 0; i < cachedEntityTypeNames.Length; i++)
                {
                    if ((newMask & (1 << i)) != 0)
                    {
                        selectedTypeNamesProp.InsertArrayElementAtIndex(idx);
                        selectedTypeNamesProp.GetArrayElementAtIndex(idx).stringValue = cachedEntityTypeNames[i];
                        idx++;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            return foldout ? lineHeight * 2 : lineHeight;
        }
    }
#endif
}