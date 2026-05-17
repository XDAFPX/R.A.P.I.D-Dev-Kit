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

        public static void InitializeCaches(object target,GameObject obj, Component[] exclude = null)
        {
            if (DEBUG_MODE)
                Debug.Log($"[UnityComponentCache] Getting the Cache for : {target.GetType()}");

            // Step 1: Collect all fields (public, non-public, instance) from this type and its base types
            var fields = GetAllFields(target.GetType());

            // Prepare exclusion set for quick lookups
            HashSet<Component> excludeSet = null;
            if (exclude != null && exclude.Length > 0)
                excludeSet = new HashSet<Component>(exclude);

            foreach (var field in fields)
            {
                bool hasComponentCache = field.IsDefined(typeof(GetComponentAttribute), false);
                if (!hasComponentCache)
                    continue;

                // Step 2: Try to get and assign the component (including interface or base types), respecting the exclude list
                var component = FindAssignableComponent(obj, field.FieldType, excludeSet);

                if (component != null)
                {
                    field.SetValue(target, component);
                    if (DEBUG_MODE)
                        Debug.Log(
                            $"[UnityComponentCache] Set {field.FieldType} to {field.Name} in {target.GetType().Name}");
                }
                else
                {
                    if (DEBUG_MODE)
                        Debug.LogError(
                            $"[UnityComponentCache] Component of type {field.FieldType} not found (or excluded) for {field.Name} in {target.GetType().Name}");
                }
            }

            // Now process properties across inheritance chain
            var properties = GetAllProperties(target.GetType());
            foreach (var prop in properties)
            {
                bool hasComponentCache = Attribute.IsDefined(prop, typeof(GetComponentAttribute));
                if (!hasComponentCache)
                    continue;
                // Skip indexers and ensure we can write
                if (prop.GetIndexParameters().Length != 0)
                    continue;
                var setter = prop.GetSetMethod(true);
                if (setter == null)
                    continue;

                var propType = prop.PropertyType;
                var component = FindAssignableComponent(obj, propType, excludeSet);

                if (component != null)
                {
                    prop.SetValue(target, component, null);
                    if (DEBUG_MODE)
                        Debug.Log($"[UnityComponentCache] Set {propType} to property {prop.Name} in {target.GetType().Name}");
                }
                else if (DEBUG_MODE)
                {
                    Debug.LogError($"[UnityComponentCache] Component of type {propType} not found (or excluded) for property {prop.Name} in {target.GetType().Name}");
                }
            }
        }

        public static void InitializeCaches(MonoBehaviour monoBehaviour)
        {
            
            if (DEBUG_MODE)
                Debug.Log($"[UnityComponentCache] Getting the Cache for : {monoBehaviour.GetType()}");

            // Step 1: Collect all fields (public, non-public, instance) from this type and its base types
            var fields = GetAllFields(monoBehaviour.GetType());

            foreach (var field in fields)
            {
                bool hasComponentCache = field.IsDefined(typeof(GetComponentAttribute), false);
                if (!hasComponentCache)
                    continue;

                // Step 2: Try to get and assign the component (including interface or base types)
                var component = FindAssignableComponent(monoBehaviour.gameObject, field.FieldType, null);
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

            // Now also process properties across inheritance
            var properties = GetAllProperties(monoBehaviour.GetType());
            foreach (var prop in properties)
            {
                bool hasComponentCache = prop.IsDefined(typeof(GetComponentAttribute), false);
                if (!hasComponentCache)
                    continue;
                if (prop.GetIndexParameters().Length != 0)
                    continue; // skip indexers
                var setter = prop.GetSetMethod(true);
                if (setter == null)
                    continue; // can’t assign

                var propType = prop.PropertyType;
                var component = FindAssignableComponent(monoBehaviour.gameObject, propType, null);
                if (component != null)
                {
                    prop.SetValue(monoBehaviour, component, null);
                    if (DEBUG_MODE)
                        Debug.Log($"[UnityComponentCache] Set {propType} to property {prop.Name} in {monoBehaviour.name}");
                }
                else if (DEBUG_MODE)
                {
                    Debug.LogError($"[UnityComponentCache] Component of type {propType} not found for property {prop.Name} in {monoBehaviour.name}");
                }
            }
        }

        /// <summary>
        /// Checks if all fields marked with [GetComponentCache] on the target have a corresponding component
        /// on the provided GameObject, without assigning/injecting anything.
        /// </summary>
        public static bool HasAllDependencies(object target, GameObject obj, Component[] exclude = null)
        {
            if (target == null || obj == null)
                return false;

            if (DEBUG_MODE)
                Debug.Log($"[UnityComponentCache] Checking dependencies for: {target.GetType()}");

            var fields = GetAllFields(target.GetType());

            // Prepare exclusion set for quick lookups
            HashSet<Component> excludeSet = null;
            if (exclude != null && exclude.Length > 0)
                excludeSet = new HashSet<Component>(exclude);

            bool allOk = true;

            foreach (var field in fields)
            {
                bool hasComponentCache = field.IsDefined(typeof(GetComponentAttribute), false);
                if (!hasComponentCache)
                    continue;

                bool satisfied = false;

                var comp = FindAssignableComponent(obj, field.FieldType, excludeSet);
                satisfied = comp != null;

                if (!satisfied)
                {
                    allOk = false;
                    if (DEBUG_MODE)
                        Debug.LogError($"[UnityComponentCache] Missing dependency: {field.FieldType} for field {field.Name} in {target.GetType().Name}");
                }
            }

            // Also check properties across inheritance
            var properties = GetAllProperties(target.GetType());
            foreach (var prop in properties)
            {
                bool hasComponentCache = prop.IsDefined(typeof(GetComponentAttribute), false);
                if (!hasComponentCache)
                    continue;
                if (prop.GetIndexParameters().Length != 0)
                    continue; // skip indexers

                bool satisfied = false;
                var propType = prop.PropertyType;

                var comp = FindAssignableComponent(obj, propType, excludeSet);
                satisfied = comp != null;

                if (!satisfied)
                {
                    allOk = false;
                    if (DEBUG_MODE)
                        Debug.LogError($"[UnityComponentCache] Missing dependency: {propType} for property {prop.Name} in {target.GetType().Name}");
                }
            }

            if (DEBUG_MODE)
                Debug.Log($"[UnityComponentCache] Dependency check result for {target.GetType().Name}: {(allOk ? "OK" : "MISSING")}");

            return allOk;
        }

        /// <summary>
        /// Convenience overload to check dependencies for a single MonoBehaviour.
        /// </summary>
        public static bool HasAllDependencies(MonoBehaviour monoBehaviour, Component[] exclude = null)
        {
            if (monoBehaviour == null)
                return false;
            return HasAllDependencies(monoBehaviour, monoBehaviour.gameObject, exclude);
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

        /// <summary>
        /// Finds a component on the GameObject that is assignable to targetType (supports interfaces and base classes).
        /// Respects the optional exclude set.
        /// </summary>
        private static Component FindAssignableComponent(GameObject obj, Type targetType, HashSet<Component> excludeSet)
        {
            if (obj == null || targetType == null) return null;

            // Fast path: try Unity's built-in lookup when no excludes
            if (excludeSet == null)
            {
                var direct = obj.GetComponent(targetType);
                if (direct != null)
                    return (Component)direct;
            }

            // Fallback and exclude-aware path: scan all components and pick the first assignable one
            var all = obj.GetComponents<Component>();
            if (all == null) return null;
            foreach (var c in all)
            {
                if (c == null) continue;
                if (excludeSet != null && excludeSet.Contains(c)) continue;
                var ct = c.GetType();
                if (targetType.IsAssignableFrom(ct))
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Recursively collects all instance properties (public & non-public) declared at each level of the inheritance chain.
        /// Includes private properties in base classes. Skips indexers.
        /// </summary>
        private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance |
                                       BindingFlags.Public |
                                       BindingFlags.NonPublic |
                                       BindingFlags.DeclaredOnly;

            while (type != null && type != typeof(MonoBehaviour) && type != typeof(object))
            {
                foreach (var prop in type.GetProperties(flags))
                {
                    if (prop == null) continue;
                    if (prop.GetIndexParameters().Length != 0) continue; // skip indexers
                    yield return prop;
                }

                type = type.BaseType;
            }
        }
    }
}