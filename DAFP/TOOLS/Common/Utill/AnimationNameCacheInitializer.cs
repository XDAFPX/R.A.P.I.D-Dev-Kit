using System;
using System.Collections.Generic;
using System.Reflection;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.Common.Utill
{
    public static class AnimationNameCacheInitializer
    {
        /// <summary>
        /// Initialize all component caches in the given MonoBehaviour.
        /// Call this method in 'Awake()', 'Start()' or any other method that is called before the first usage of the cached component.
        /// </summary>
        public static bool DEBUG_MODE = false;

        public static void InitializeCaches(MonoBehaviour monoBehaviour)
        {
            if (DEBUG_MODE)
                Debug.Log($"[AnimationNameCacheInitializer] Getting the Cache for : {monoBehaviour.GetType()}");

            // Step 1: Collect all fields (public, non-public, instance) from this type and its base types
            var fields = GetAllFields(monoBehaviour.GetType());

            foreach (var field in fields)
            {
                bool has_animation_hash = Attribute.IsDefined(field, typeof(AnimationNameAttribute));
                if (!has_animation_hash)
                    continue;

                var attr = field.GetCustomAttribute<AnimationNameAttribute>();
                if (attr != null)
                {
                    string animationName = attr.Name;
                    int hash = Animator.StringToHash(animationName);
                    field.SetValue(monoBehaviour, hash);
                    if (DEBUG_MODE)
                        Debug.Log(
                            $"[AnimationNameCacheInitializer] Cached hash for “{animationName}” into {field.Name}");
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