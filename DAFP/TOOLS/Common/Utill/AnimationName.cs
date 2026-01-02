using UnityEngine;

namespace DAFP.TOOLS.Common.Utill
{
    /// <summary>
    /// Use this attribute to mark an int field that should be cached using Animator.StringToHash.
    /// **Cached in Runtime**: 'AnimationNameCacheInitializer.InitializeCaches()' must be called before start using the cached animation
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class AnimationNameAttribute : PropertyAttribute
    {
        public string Name { get; }

        public AnimationNameAttribute(string name)
        {
            Name = name;
        }
    }
}