using System;
using System.Collections.Generic;
using UnityEngine;

namespace DAFP.TOOLS.Common.Utill
{
 /// <summary>
    /// Generic loader for any JSON file in Resources whose root format is:
    /// {
    ///   "items": [ /* array of T */ ]
    /// }
    /// Usage:
    ///   var list = JsonEnumLoader.Load<string>("DamageTypes");
    /// </summary>
    public static class JsonEnumLoader
    {
        [Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }

        /// <summary>
        /// Loads a JSON file named <resourceName>.json from Resources,
        /// expecting it to have an "items" array of T.
        /// </summary>
        /// <typeparam name="T">Type of each entry (e.g. string, int, etc.).</typeparam>
        /// <param name="resourceName">Path under Resources (without ".json").</param>
        /// <returns>List of T, or empty list on error.</returns>
        public static IReadOnlyList<T> Load<T>(string resourceName)
        {
            var ta = Resources.Load<TextAsset>(resourceName);
            if (ta == null)
            {
                Debug.LogWarning($"[JsonEnumLoader] Resource '{resourceName}' not found.");
                return Array.Empty<T>();
            }

            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(ta.text);
                if (wrapper?.items != null)
                    return wrapper.items;
                return Array.Empty<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonEnumLoader] Failed to parse '{resourceName}': {ex}");
                return Array.Empty<T>();
            }
        }
    }

}