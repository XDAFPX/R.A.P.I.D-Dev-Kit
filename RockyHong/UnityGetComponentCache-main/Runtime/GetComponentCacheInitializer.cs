using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityGetComponentCache
{
    public static class GetComponentCacheInitializer
    {
        /// <summary>
        /// Initialize all component caches in the given MonoBehaviour.
        /// Call this method in 'Awake()', 'Start()' or any other method that is called before the first usage of the cached component.
        /// </summary>
        public static bool DEBUG_MODE = false;

        public static void InitializeCaches(MonoBehaviour monoBehaviour)
        {
            if (DEBUG_MODE)
                Debug.Log($"[UnityComponentCache] Getting the Cache for : {monoBehaviour.GetType()}");

            // Step 1: Collect all fields (public, non-public, instance) from this type and its base types
            var fields = GetAllFields(monoBehaviour.GetType());

            foreach (var field in fields)
            {
                bool hasComponentCache = Attribute.IsDefined(field, typeof(GetComponentCacheAttribute));
                if (!hasComponentCache)
                    continue;

                // Step 2: Try to get and assign the component
                var component = monoBehaviour.GetComponent(field.FieldType);
                if (component != null)
                {
                    field.SetValue(monoBehaviour, component);
                    if (DEBUG_MODE)
                        Debug.Log(
                            $"[UnityComponentCache] Set {field.FieldType} to {field.Name} in {monoBehaviour.name}");
                }
                else
                {
                    if (DEBUG_MODE)
                        Debug.LogError(
                            $"[UnityComponentCache] Component of type {field.FieldType} not found for {field.Name} in {monoBehaviour.name}");
                }
            }
        }

        /// <summary>
        /// Recursively collects all instance fields (public & non-public) declared at each level of the inheritance chain.
        /// This includes private fields in base classes.
        /// </summary>
        private static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance |
                                       BindingFlags.Public |
                                       BindingFlags.NonPublic |
                                       BindingFlags.DeclaredOnly;

            while (type != null && type != typeof(MonoBehaviour) && type != typeof(object))
            {
                foreach (var field in type.GetFields(flags))
                    yield return field;

                type = type.BaseType;
            }
        }
    }
}