using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DAFP.TOOLS.Common.Utill
{
    public static class MethodGetter
    {
        /// <summary>
        /// Recursively collects all instance methods (public & non-public) declared at each level of the inheritance chain.
        /// This includes private methods in base classes.
        /// </summary>
        public static IEnumerable<MethodInfo> GetAllMethods(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance |
                                       BindingFlags.Public |
                                       BindingFlags.NonPublic |
                                       BindingFlags.DeclaredOnly;

            while (type != null && type != typeof(MonoBehaviour) && type != typeof(object))
            {
                foreach (var method in type.GetMethods(flags))
                    yield return method;

                type = type.BaseType;
            }
        }
    }
}