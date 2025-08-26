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
            var fields = FieldGetter.GetAllFields(monoBehaviour.GetType());

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
    }
}