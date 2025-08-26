using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DAFP.TOOLS.Common.Utill
{
    public static class FieldGetter
    {
        /// <summary>
        /// Recursively collects all instance fields (public & non-public) declared at each level of the inheritance chain.
        /// This includes private fields in base classes.
        /// </summary>
        public static IEnumerable<FieldInfo> GetAllFields(Type type)
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