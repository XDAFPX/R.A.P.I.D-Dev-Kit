using System;
using System.Collections.Generic;
using System.Reflection;
using DAFP.TOOLS.Common.Utill;
using NRandom;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Common
{
    public class UniversalComponentRandomizer : MonoBehaviour, IRandomizer
    {
        public Component target;
        public List<FieldRandomSetting> settings = new List<FieldRandomSetting>();

        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


        public void Randomize(IRandom rng)
        {
            if (target == null) return;

            foreach (var setting in settings)
            {
                if (!setting.enabled) continue;
                ApplySetting(target, setting, rng);
            }
        }

        private void ApplySetting(object root, FieldRandomSetting setting, IRandom rng)
        {
            Type fieldType = Type.GetType(setting.fieldTypeAssemblyQualifiedName);
            if (fieldType == null) return;

            object randomValue = GenerateRandomValue(fieldType, setting, rng);
            if (randomValue == null) return;

            ReflectionPath.SetValue(root, setting.path, randomValue, FieldFlags);
        }

        private object GenerateRandomValue(Type fieldType, FieldRandomSetting s, IRandom rng)
        {
            switch (FieldKindUtility.GetKind(fieldType))
            {
                case FieldKind.Float:
                    return rng.NextFloat(s.minFloat, s.maxFloat);
                case FieldKind.Int:
                    return rng.NextInt(s.minInt, s.maxInt + 1);
                case FieldKind.UInt:
                    return rng.NextUInt(s.minUInt, s.maxUInt + 1);
                case FieldKind.Bool:
                    return rng.NextBool();
                case FieldKind.Vector2:
                    return new Vector2(
                        rng.Range(s.minVector.x, s.maxVector.x),
                        rng.Range(s.minVector.y, s.maxVector.y));

                case FieldKind.Vector3:
                    return new Vector3(
                        rng.Range(s.minVector.x, s.maxVector.x),
                        rng.Range(s.minVector.y, s.maxVector.y),
                        rng.Range(s.minVector.z, s.maxVector.z));

                case FieldKind.Vector4:
                    return new Vector4(
                        rng.Range(s.minVector.x, s.maxVector.x),
                        rng.Range(s.minVector.y, s.maxVector.y),
                        rng.Range(s.minVector.z, s.maxVector.z),
                        rng.Range(s.minVector.w, s.maxVector.w));

                case FieldKind.Color:
                    return Color.Lerp(s.minColor, s.maxColor, rng.Value());

                default:
                    return null;
            }
        }
    }

    [Serializable]
    public class FieldRandomSetting
    {
        /// Dot-separated path from the target root, e.g. "emission.rateOverTime"
        public string path;

        public string displayName;
        public string fieldTypeAssemblyQualifiedName;
        public bool enabled;

        public float minFloat, maxFloat = 1f;
        public int minInt, maxInt = 1;
        public uint minUInt, maxUInt = 1;
        [Range(0f, 1f)] public float boolChance = 0.5f;
        public Vector4 minVector, maxVector = Vector4.one;
        public Color minColor = Color.black, maxColor = Color.white;
    }

    public enum FieldKind
    {
        Unsupported,
        Float,
        Int,
        UInt,
        Bool,
        Vector2,
        Vector3,
        Vector4,
        Color
    }

    public static class FieldKindUtility
    {
        public static FieldKind GetKind(Type t)
        {
            if (t == typeof(float)) return FieldKind.Float;
            if (t == typeof(int)) return FieldKind.Int;
            if (t == typeof(uint)) return FieldKind.UInt;
            if (t == typeof(bool)) return FieldKind.Bool;
            if (t == typeof(Vector2)) return FieldKind.Vector2;
            if (t == typeof(Vector3)) return FieldKind.Vector3;
            if (t == typeof(Vector4)) return FieldKind.Vector4;
            if (t == typeof(Color)) return FieldKind.Color;
            return FieldKind.Unsupported;
        }

        public static bool IsSupportedLeaf(Type t) => GetKind(t) != FieldKind.Unsupported;

        /// True for plain classes/structs marked [Serializable] that aren't
        /// Unity Objects and aren't one of our directly-supported leaf types.
        public static bool IsNestedSerializable(Type t)
        {
            if (t == null) return false;
            if (IsSupportedLeaf(t)) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(t)) return false;
            if (t.IsArray) return false;
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && t != typeof(string)) return false;
            if (t == typeof(string)) return false;
            if (!t.IsClass && !t.IsValueType) return false;

            return Attribute.IsDefined(t, typeof(SerializableAttribute));
        }
    }

    /// <summary>
    /// Wraps either a FieldInfo or a PropertyInfo behind one interface, so
    /// callers don't need to care which one a given member actually is.
    /// This is what makes properties (like Transform.position) work the same
    /// way as [SerializeField] fields everywhere else in this system.
    /// </summary>
    public readonly struct MemberAccessor
    {
        private readonly FieldInfo _field;
        private readonly PropertyInfo _property;

        public MemberAccessor(FieldInfo field)
        {
            _field = field;
            _property = null;
        }

        public MemberAccessor(PropertyInfo property)
        {
            _field = null;
            _property = property;
        }

        public bool IsValid => _field != null || _property != null;
        public string Name => _field?.Name ?? _property?.Name;
        public Type MemberType => _field?.FieldType ?? _property?.PropertyType;

        public object GetValue(object obj)
        {
            return _field != null ? _field.GetValue(obj) : _property.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            if (_field != null) _field.SetValue(obj, value);
            else _property.SetValue(obj, value);
        }
    }

    /// <summary>
    /// Curated list of settable Transform properties to expose, since
    /// Transform's real backing data isn't reachable via normal field
    /// reflection (it's native-backed, not [SerializeField] fields).
    /// Works for RectTransform too, since it derives from Transform.
    /// </summary>
    public static class TransformMemberWhitelist
    {
        private static readonly string[] PropertyNames =
        {
            "position",
            "localPosition",
            "eulerAngles",
            "localEulerAngles",
            "localScale",
            "forward",
            "up",
            "right"
        };

        public static bool AppliesTo(Type type) => typeof(Transform).IsAssignableFrom(type);

        public static IEnumerable<PropertyInfo> GetWhitelistedProperties()
        {
            foreach (var name in PropertyNames)
            {
                var prop = typeof(Transform).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && prop.CanRead && prop.CanWrite)
                    yield return prop;
            }
        }
    }

    /// <summary>
    /// Walks/writes a dot-separated member path via reflection. Handles
    /// value-type (struct) chains correctly by re-assigning each parent
    /// after modifying its child, since structs are copied by value.
    /// Supports both fields and properties (see MemberAccessor).
    /// </summary>
    public static class ReflectionPath
    {
        public static object GetValue(object root, string path, BindingFlags flags)
        {
            object current = root;
            foreach (var part in path.Split('.'))
            {
                if (current == null) return null;
                var member = FindMember(current.GetType(), part, flags);
                if (!member.IsValid) return null;
                current = member.GetValue(current);
            }

            return current;
        }

        public static void SetValue(object root, string path, object value, BindingFlags flags)
        {
            var parts = path.Split('.');
            SetRecursive(root, parts, 0, value, flags);
        }

        private static object SetRecursive(object obj, string[] parts, int index, object value, BindingFlags flags)
        {
            if (obj == null) return null;

            var member = FindMember(obj.GetType(), parts[index], flags);
            if (!member.IsValid) return obj;

            if (index == parts.Length - 1)
            {
                member.SetValue(obj, value);
                return obj;
            }

            object child = member.GetValue(obj);
            if (child == null && member.MemberType.IsClass)
            {
                child = Activator.CreateInstance(member.MemberType);
            }

            object updatedChild = SetRecursive(child, parts, index + 1, value, flags);

            // Required even for reference types (harmless there) and essential
            // for value types (structs), since child was a copy.
            member.SetValue(obj, updatedChild);

            return obj;
        }

        // Walks up the inheritance chain — Unity serializes private
        // [SerializeField] fields declared on base classes too. Falls back
        // to a matching settable property (e.g. Transform.position) if no
        // field is found.
        private static MemberAccessor FindMember(Type type, string name, BindingFlags flags)
        {
            var t = type;
            while (t != null)
            {
                var field = t.GetField(name, flags | BindingFlags.DeclaredOnly);
                if (field != null) return new MemberAccessor(field);

                var prop = t.GetProperty(name, flags | BindingFlags.DeclaredOnly);
                if (prop != null && prop.CanRead && prop.CanWrite) return new MemberAccessor(prop);

                t = t.BaseType;
            }

            return default;
        }
    }
}// after