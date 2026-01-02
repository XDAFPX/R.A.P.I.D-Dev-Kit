using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public static class StatInjector
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic |
                                                   BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        // Internal model for missing required injections
        private class MissingRequired
        {
            public string Path;
            public Type DeclaredType;
            public Type ValueType;
            public object FallbackObject;
            public string FallbackFieldName;
        }

        public static bool InjectAndValidateStats(IEntity entity)
        {
            if (entity.Stats == null)
                return false;
            var _type = entity.GetType();

            // 1) Inject only (no creation); collect missing required with full metadata
            var _fields = _type.GetFields(BINDING_FLAGS)
                .Where(f => f.GetCustomAttribute<InjectStatAttribute>() != null)
                .ToArray();
            var _properties = _type.GetProperties(BINDING_FLAGS)
                .Where(p => p.GetCustomAttribute<InjectStatAttribute>() != null && p.CanWrite)
                .ToArray();

            var _missing = new List<MissingRequired>();
            bool _allInjected = true;

            foreach (var _field in _fields)
            {
                var _inject = _field.GetCustomAttribute<InjectStatAttribute>();
                var _required = _field.GetCustomAttribute<RequireStatAttribute>() != null;

                if (!try_inject_field(entity, _field, _inject, _required))
                {
                    _allInjected = false;
                    if (_required)
                    {
                        var _statValueType = get_stat_value_type(_field.FieldType);
                        _missing.Add(new MissingRequired
                        {
                            Path = _inject.StatName,
                            DeclaredType = _field.FieldType,
                            ValueType = _statValueType,
                            FallbackObject = _inject.FallbackObject,
                            FallbackFieldName = _inject.FallbackFieldName
                        });
                    }
                }
            }

            foreach (var _property in _properties)
            {
                var _inject = _property.GetCustomAttribute<InjectStatAttribute>();
                var _required = _property.GetCustomAttribute<RequireStatAttribute>() != null;

                if (!try_inject_property(entity, _property, _inject, _required))
                {
                    _allInjected = false;
                    if (_required)
                    {
                        var _statValueType = get_stat_value_type(_property.PropertyType);
                        _missing.Add(new MissingRequired
                        {
                            Path = _inject.StatName,
                            DeclaredType = _property.PropertyType,
                            ValueType = _statValueType,
                            FallbackObject = _inject.FallbackObject,
                            FallbackFieldName = _inject.FallbackFieldName
                        });
                    }
                }
            }

            if (_missing.Count == 0)
            {
                return _allInjected;
            }

            // 2) Build a quick lookup of required declarations by path for choosing parent types
            var _requiredByPath = new Dictionary<string, (Type declared, Type value)>(StringComparer.OrdinalIgnoreCase);
            foreach (var _m in _missing)
            {
                if (!_requiredByPath.ContainsKey(_m.Path))
                {
                    _requiredByPath[_m.Path] = (_m.DeclaredType, _m.ValueType);
                }
            }

            // Also include already-declared required members that did inject successfully
            foreach (var _field in _fields)
            {
                var _req = _field.GetCustomAttribute<RequireStatAttribute>() != null;
                if (!_req) continue;
                var _inject = _field.GetCustomAttribute<InjectStatAttribute>();
                if (_inject == null) continue;
                var _path = _inject.StatName;
                if (string.IsNullOrWhiteSpace(_path)) continue;
                var _valueType = get_stat_value_type(_field.FieldType);
                if (!_requiredByPath.ContainsKey(_path))
                    _requiredByPath[_path] = (_field.FieldType, _valueType);
            }

            foreach (var _property in _properties)
            {
                var _req = _property.GetCustomAttribute<RequireStatAttribute>() != null;
                if (!_req) continue;
                var _inject = _property.GetCustomAttribute<InjectStatAttribute>();
                if (_inject == null) continue;
                var _path = _inject.StatName;
                if (string.IsNullOrWhiteSpace(_path)) continue;
                var _valueType = get_stat_value_type(_property.PropertyType);
                if (!_requiredByPath.ContainsKey(_path))
                    _requiredByPath[_path] = (_property.PropertyType, _valueType);
            }

            // 3) Create hierarchy for each missing required path
            foreach (var _m in _missing)
            {
                try
                {
                    ensure_path_and_create_leaf(entity.Stats, _m.Path, _m.DeclaredType, _m.ValueType, _m.FallbackObject, _requiredByPath);
                }
                catch (Exception _e)
                {
                    Debug.LogError($"Failed to ensure path '{_m.Path}': {_e.Message}", entity.GetWorldRepresentation());
                }
            }

            // After creation, mark dirty once
            entity.Stats.MarkAsDirty();

            // 4) Re-inject only
            bool _reinjectOk = re_inject_after_adding_stats(entity, _fields, _properties);
            return _reinjectOk;
        }

        /// <summary>
        /// Ensures the full path exists; creates intermediate parents with GenericWhiteBoard<TLeaf>
        /// and finally creates/returns the leaf if missing. Parents use same value type as the leaf.
        /// </summary>
        private static IStatBase ensure_path_and_create_leaf(StatContainer container, string path,
            Type declaredLeafType, Type leafValueType, object fallbackObject,
            Dictionary<string, (Type declared, Type value)> requiredByPath)
        {
            if (container == null) return null;
            if (string.IsNullOrWhiteSpace(path)) return null;

            // If the whole path already exists, nothing to create
            var _existing = resolve_stat_by_path(container, path);
            if (_existing != null)
                return _existing;

            var _segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (_segments.Length == 0) return null;

            IStatBase _currentParent = null;

            // Ensure each parent segment
            for (int _i = 0; _i < _segments.Length; _i++)
            {
                string _seg = _segments[_i];
                bool _isLeaf = (_i == _segments.Length - 1);

                // Try find existing at current level
                IStatBase _found = (_currentParent == null)
                    ? try_find_root_by_name(container, _seg)
                    : try_find_child_by_name(_currentParent, _seg);

                if (_found != null)
                {
                    _currentParent = _found;
                    continue;
                }

                // Create missing node
                IStatBase _created;
                if (_isLeaf)
                {
                    // Create leaf using declaredLeafType or fallback to GenericWhiteBoard
                    var _concreteLeaf = determine_concrete_stat_type(declaredLeafType, leafValueType);
                    _created = (fallbackObject != null)
                        ? create_stat_with_fallback(_concreteLeaf, leafValueType, fallbackObject)
                        : (IStatBase)Activator.CreateInstance(_concreteLeaf);
                }
                else
                {
                    // Create intermediate parent as GenericWhiteBoard<TLeaf>
                    if (leafValueType == null)
                    {
                        throw new ArgumentException(
                            $"Cannot determine value type T for path '{path}'. Declared leaf type: '{declaredLeafType?.Name ?? "<null>"}'. Ensure the member type implements IStat<T> or derives WhiteBoard<T>.");
                    }

                    var _parentType = typeof(GenericWhiteBoard<>).MakeGenericType(leafValueType);
                    _created = (IStatBase)Activator.CreateInstance(_parentType);
                }

                _created.Name = _seg;

                // Attach
                if (_currentParent == null)
                {
                    // Add as root
                    // Avoid duplicate add if somehow appeared
                    if (try_find_root_by_name(container, _seg) == null)
                        container.Add(_created);
                }
                else
                {
                    add_child_to_parent(_currentParent, _created);
                }

                _currentParent = _created;
            }

            // Return the leaf (should resolve now)
            return resolve_stat_by_path(container, path);
        }

        /// <summary>
        /// Re-injects stats after adding missing ones, without recursion
        /// </summary>
        private static bool re_inject_after_adding_stats(IEntity entity, IEnumerable<FieldInfo> fields,
            IEnumerable<PropertyInfo> properties)
        {
            bool _allSuccessful = true;

            // Re-inject fields that were missing
            foreach (var _field in fields)
            {
                var _injectAttr = _field.GetCustomAttribute<InjectStatAttribute>();
                var _isRequired = _field.GetCustomAttribute<RequireStatAttribute>() != null;

                // Only re-inject if field is still null/not set
                if (_field.GetValue(entity) == null)
                {
                    if (!try_inject_field(entity, _field, _injectAttr, _isRequired))
                    {
                        _allSuccessful = false;
                    }
                }
            }

            // Re-inject properties that were missing
            foreach (var _property in properties)
            {
                var _injectAttr = _property.GetCustomAttribute<InjectStatAttribute>();
                var _isRequired = _property.GetCustomAttribute<RequireStatAttribute>() != null;

                // Only re-inject if property is still null/not set
                if (_property.GetValue(entity) == null)
                {
                    if (!try_inject_property(entity, _property, _injectAttr, _isRequired))
                    {
                        _allSuccessful = false;
                    }
                }
            }

            return _allSuccessful;
        }

        /// <summary>
        /// Creates a stat instance with a fallback value
        /// </summary>
        private static IStatBase create_stat_with_fallback(Type concreteType, Type statValueType, object fallbackObject)
        {
            // Try to find a constructor that accepts the value type
            var _valueConstructor = concreteType.GetConstructor(new[] { statValueType });

            if (_valueConstructor != null && is_value_type_compatible(fallbackObject, statValueType))
            {
                // Use constructor that takes a value
                var _convertedValue = Convert.ChangeType(fallbackObject, statValueType);
                return (IStatBase)_valueConstructor.Invoke(new[] { _convertedValue });
            }

            // Otherwise, create with default constructor and set Value property
            var _instance = (IStatBase)Activator.CreateInstance(concreteType);

            // Try to set the Value property
            var _valueProperty = concreteType.GetProperty("Value");
            if (_valueProperty != null && _valueProperty.CanWrite)
            {
                try
                {
                    var _convertedValue = Convert.ChangeType(fallbackObject, statValueType);
                    _valueProperty.SetValue(_instance, _convertedValue);
                }
                catch (Exception _e)
                {
                    Debug.LogWarning($"Failed to set fallback value on {concreteType.Name}: {_e.Message}");
                }
            }

            return _instance;
        }

        /// <summary>
        /// Checks if a fallback object can be converted to the target type
        /// </summary>
        private static bool is_value_type_compatible(object value, Type targetType)
        {
            if (value == null)
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

            var _valueType = value.GetType();

            // Direct assignable
            if (targetType.IsAssignableFrom(_valueType))
                return true;

            // Can be converted
            try
            {
                Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the value type T from IStat<T>
        /// </summary>
        private static Type get_stat_value_type(Type statType)
        {
            if (is_stat_type(statType, out var _valueType))
            {
                return _valueType;
            }

            throw new InvalidOperationException($"Type {statType.Name} does not implement IStat<T>");
        }

        /// <summary>
        /// Determines the concrete type to instantiate for a missing stat.
        /// If the field/property is declared as a concrete type (e.g., HealthStat or MyCustomStat<int>), use that.
        /// If it's declared as IStat<T>, default to GenericWhiteBoard<T>.
        /// </summary>
        private static Type determine_concrete_stat_type(Type declaredType, Type statValueType)
        {
            if (statValueType == null)
                throw new ArgumentException(
                    "statValueType cannot be null when determining concrete stat type. This usually means the member type does not implement IStat<T> nor derive WhiteBoard<T>.");

            // If no declared type, use default implementation
            if (declaredType == null)
            {
                return typeof(GenericWhiteBoard<>).MakeGenericType(statValueType);
            }

            // If the declared type is an interface (e.g., IStat<T>), use a default implementation
            if (declaredType.IsInterface)
            {
                return typeof(GenericWhiteBoard<>).MakeGenericType(statValueType);
            }

            // If it's a concrete class that implements IStat<T>, use that type directly
            // This handles both generic (MyCustomStat<int>) and non-generic (HealthStat : Stat<int>) cases
            if (declaredType.IsClass && !declaredType.IsAbstract)
            {
                var _istatInterface = declaredType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStat<>));

                if (_istatInterface != null)
                {
                    // Use the declared type as-is (whether it's generic or not)
                    return declaredType;
                }
            }

            // Fallback to GenericWhiteBoard
            return typeof(GenericWhiteBoard<>).MakeGenericType(statValueType);
        }

        /// <summary>
        /// Checks if a type is IStat<T> or implements IStat<T>, and returns the value type T.
        /// </summary>
        private static bool is_stat_type(Type type, out Type statValueType)
        {
            statValueType = null;

            if (type == null)
                return false;

            // Check if it's directly IStat<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStat<>))
            {
                statValueType = type.GetGenericArguments()[0];
                return true;
            }

            // Check if it implements IStat<T>
            var _istatInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStat<>));

            if (_istatInterface != null)
            {
                statValueType = _istatInterface.GetGenericArguments()[0];
                return true;
            }

            // Fallback: try to detect generic base WhiteBoard<T>
            var t = type;
            while (t != null)
            {
                if (t.IsGenericType)
                {
                    var def = t.GetGenericTypeDefinition();
                    if (def == typeof(WhiteBoard<>))
                    {
                        statValueType = t.GetGenericArguments()[0];
                        return true;
                    }
                }

                t = t.BaseType;
            }

            return false;
        }

        private static bool try_inject_field(IEntity entity, FieldInfo field, InjectStatAttribute attr, bool isRequired)
        {
            try
            {
                var _fieldType = field.FieldType;

                // Handle IStat<T> or concrete implementations
                if (is_stat_type(_fieldType, out var _statValueType))
                {
                    object _stat = get_stat(entity.Stats, attr.StatName, _statValueType, _fieldType);

                    // Try fallback field if primary stat not found
                    if (_stat == null && !string.IsNullOrEmpty(attr.FallbackFieldName))
                    {
                        _stat = get_fallback_value(entity, attr.FallbackFieldName, _fieldType);

                        if (_stat != null)
                        {
                            Debug.Log(
                                $"Stat '{attr.StatName}' not found for field '{field.Name}' on {entity.Name}, using fallback field '{attr.FallbackFieldName}'",
                                entity.GetWorldRepresentation());
                        }
                    }

                    if (_stat == null)
                    {
                        if (isRequired)
                        {
                            var _fallbackMsg = !string.IsNullOrEmpty(attr.FallbackFieldName)
                                ? $" (fallback field '{attr.FallbackFieldName}' also unavailable)"
                                : "";
                            // Debug.LogError(
                            //     $"[REQUIRED] Stat '{attr.StatName}' not found for field '{field.Name}' on {entity.Name}{_fallbackMsg}",
                            //     entity.GetWorldRepresentation());
                            return false;
                        }
                        else
                        {
                            var _fallbackMsg = !string.IsNullOrEmpty(attr.FallbackFieldName)
                                ? $" (fallback field '{attr.FallbackFieldName}' also unavailable)"
                                : "";
                            Debug.LogWarning(
                                $"[OPTIONAL] Stat '{attr.StatName}' not found for field '{field.Name}' on {entity.Name}{_fallbackMsg}",
                                entity.GetWorldRepresentation());
                            return true;
                        }
                    }

                    field.SetValue(entity, _stat);
                    return true;
                }

                Debug.LogWarning(
                    $"Field '{field.Name}' has unsupported type for stat injection: {_fieldType.Name}. Expected IStat<T> or a concrete implementation.",
                    entity.GetWorldRepresentation());
                return !isRequired;
            }
            catch (Exception _e)
            {
                if (isRequired)
                {
                    Debug.LogError(
                        $"Failed to inject REQUIRED stat '{attr.StatName}' into field '{field.Name}' on {entity.Name}: {_e.Message}",
                        entity.GetWorldRepresentation());
                    return false;
                }

                Debug.LogWarning(
                    $"Failed to inject optional stat '{attr.StatName}' into field '{field.Name}' on {entity.Name}: {_e.Message}",
                    entity.GetWorldRepresentation());
                return true;
            }
        }

        private static bool try_inject_property(IEntity entity, PropertyInfo property, InjectStatAttribute attr,
            bool isRequired)
        {
            try
            {
                var _propertyType = property.PropertyType;

                if (is_stat_type(_propertyType, out var _statValueType))
                {
                    object _stat = get_stat(entity.Stats, attr.StatName, _statValueType, _propertyType);

                    // Try fallback field if primary stat not found
                    if (_stat == null && !string.IsNullOrEmpty(attr.FallbackFieldName))
                    {
                        _stat = get_fallback_value(entity, attr.FallbackFieldName, _propertyType);

                        if (_stat != null)
                        {
                            Debug.Log(
                                $"Stat '{attr.StatName}' not found for property '{property.Name}' on {entity.Name}, using fallback field '{attr.FallbackFieldName}'",
                                entity.GetWorldRepresentation());
                        }
                    }

                    if (_stat == null)
                    {
                        if (isRequired)
                        {
                            var _fallbackMsg = !string.IsNullOrEmpty(attr.FallbackFieldName)
                                ? $" (fallback field '{attr.FallbackFieldName}' also unavailable)"
                                : "";
                            Debug.LogError(
                                $"[REQUIRED] Stat '{attr.StatName}' not found for property '{property.Name}' on {entity.Name}{_fallbackMsg}",
                                entity.GetWorldRepresentation());
                            return false;
                        }
                        else
                        {
                            var _fallbackMsg = !string.IsNullOrEmpty(attr.FallbackFieldName)
                                ? $" (fallback field '{attr.FallbackFieldName}' also unavailable)"
                                : "";
                            Debug.LogWarning(
                                $"[OPTIONAL] Stat '{attr.StatName}' not found for property '{property.Name}' on {entity.Name}{_fallbackMsg}",
                                entity.GetWorldRepresentation());
                            return true;
                        }
                    }

                    property.SetValue(entity, _stat);
                    return true;
                }

                Debug.LogWarning(
                    $"Property '{property.Name}' has unsupported type for stat injection: {_propertyType.Name}. Expected IStat<T> or a concrete implementation.",
                    entity.GetWorldRepresentation());
                return !isRequired;
            }
            catch (Exception _e)
            {
                if (isRequired)
                {
                    Debug.LogError(
                        $"Failed to inject REQUIRED stat '{attr.StatName}' into property '{property.Name}' on {entity.Name}: {_e.Message}",
                        entity.GetWorldRepresentation());
                    return false;
                }

                Debug.LogWarning(
                    $"Failed to inject optional stat '{attr.StatName}' into property '{property.Name}' on {entity.Name}: {_e.Message}",
                    entity.GetWorldRepresentation());
                return true;
            }
        }

        private static object get_stat(StatContainer stats, string statName, Type statValueType, Type declaredType)
        {
            try
            {
                // Path-based resolution: e.g., "Damage/PunchDamage"
                if (!string.IsNullOrEmpty(statName) && statName.Contains("/"))
                {
                    var _node = resolve_stat_by_path(stats, statName);
                    if (_node == null)
                    {
                        return null;
                    }

                    // Type compatibility check
                    if (!declaredType.IsInstanceOfType(_node))
                    {
                        // For IStat<T> declared as interface, allow compatible implementations
                        if (is_stat_type(_node.GetType(), out var _nodeValueType))
                        {
                            if (_nodeValueType != statValueType)
                            {
                                Debug.LogWarning(
                                    $"Stat path '{statName}' resolved to type with value {_nodeValueType.Name}, expected {statValueType.Name}");
                                return null;
                            }
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"Stat path '{statName}' resolved to incompatible type {_node.GetType().Name} for {declaredType.Name}");
                            return null;
                        }
                    }

                    return _node;
                }

                // Fallback to flat-name lookup via StatContainer.Has(...)
                if (stats != null && stats.Has(statName, out IStatBase _foundBase))
                {
                    // Type compatibility check
                    if (!declaredType.IsInstanceOfType(_foundBase))
                    {
                        // When declared as IStat<T>, allow compatible implementations with matching T
                        if (is_stat_type(_foundBase.GetType(), out var _nodeValueType))
                        {
                            if (_nodeValueType != statValueType)
                            {
                                Debug.LogWarning(
                                    $"Stat '{statName}' found but has value type {_nodeValueType.Name}, expected {statValueType.Name}");
                                return null;
                            }
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"Stat '{statName}' found but is incompatible type {_foundBase.GetType().Name} for {declaredType.Name}");
                            return null;
                        }
                    }

                    return _foundBase;
                }

                // Not found
                return null;
            }
            catch (Exception _e)
            {
                Debug.LogWarning($"Exception getting stat '{statName}': {_e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolves a stat by a slash-separated path starting from top-level stats in the container.
        /// Example: "Damage/PunchDamage/Bonus".
        /// </summary>
        private static IStatBase resolve_stat_by_path(StatContainer container, string path)
        {
            if (container == null || string.IsNullOrWhiteSpace(path))
                return null;

            var _segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (_segments.Length == 0)
                return null;

            var _roots = get_root_stats(container);
            if (_roots == null)
                return null;

            IEnumerable<IStatBase> _currentLevel = _roots;
            IStatBase _current = null;

            for (int _i = 0; _i < _segments.Length; _i++)
            {
                string _seg = _segments[_i];
                var _matches = _currentLevel
                    .Where(s => s != null && !string.IsNullOrEmpty(s.Name) &&
                                string.Equals(s.Name, _seg, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (_matches.Count == 0)
                {
                    return null; // missing segment
                }

                if (_matches.Count > 1)
                {
                    Debug.LogWarning(
                        $"Ambiguous stat path segment '{_seg}' at '{string.Join("/", _segments.Take(_i))}'. {_matches.Count} matches found.");
                    return null;
                }

                _current = _matches[0];
                _currentLevel = get_children(_current);
            }

            return _current;
        }

        private static IEnumerable<IStatBase> get_root_stats(StatContainer container)
        {
            try
            {
                var _field = typeof(StatContainer).GetField("Stats", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_field == null)
                    return null;

                var _raw =
                    _field.GetValue(
                        container) as System.Collections.IEnumerable; // List<SerializableInterface<IStatBase>>
                if (_raw == null)
                    return null;

                var _list = new List<IStatBase>();
                foreach (var _item in _raw)
                {
                    // Each item is SerializableInterface<IStatBase> with property Value
                    var _prop = _item.GetType().GetProperty("Value");
                    if (_prop == null) continue;
                    var _val = _prop.GetValue(_item) as IStatBase;
                    if (_val != null) _list.Add(_val);
                }

                return _list;
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<IStatBase> get_children(IStatBase owner)
        {
            if (owner == null)
                yield break;

            // Try to read WhiteBoard<>.Children directly to avoid null DelegateSet usage
            var _type = owner.GetType();
            // Walk inheritance to find WhiteBoard<>
            Type _t = _type;
            while (_t != null)
            {
                if (_t.IsGenericType && _t.GetGenericTypeDefinition().Name.StartsWith("WhiteBoard"))
                {
                    var _childrenField = _t.GetField("Children",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_childrenField != null)
                    {
                        var _listObj =
                            _childrenField
                                    .GetValue(owner) as
                                System.Collections.IEnumerable; // List<SerializableInterface<IStatBase>>
                        if (_listObj != null)
                        {
                            foreach (var _item in _listObj)
                            {
                                var _valProp = _item.GetType().GetProperty("Value");
                                if (_valProp == null) continue;
                                if (_valProp.GetValue(_item) is IStatBase _child && _child != null)
                                    yield return _child;
                            }

                            yield break; // already enumerated
                        }
                    }
                }

                _t = _t.BaseType;
            }

            // Fallback to IOwner<IStatBase>.Pets enumeration
            if (owner is DAFP.TOOLS.Common.IOwner<IStatBase> _o && _o.Pets != null)
            {
                foreach (var _pet in _o.Pets)
                {
                    if (_pet is IStatBase _child)
                        yield return _child;
                }
            }
        }

        private static IStatBase try_find_root_by_name(StatContainer container, string name)
        {
            if (container == null || string.IsNullOrEmpty(name)) return null;
            var _roots = get_root_stats(container);
            if (_roots == null) return null;
            return _roots.FirstOrDefault(s =>
                s != null && !string.IsNullOrEmpty(s.Name) &&
                string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static IStatBase try_find_child_by_name(IStatBase parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name)) return null;
            return get_children(parent).FirstOrDefault(s =>
                s != null && !string.IsNullOrEmpty(s.Name) &&
                string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static void add_child_to_parent(IStatBase parent, IStatBase child)
        {
            if (parent == null || child == null) return;

            var _list = ensure_children_list(parent);
            if (_list == null) return;

            // Create SerializableInterface<IStatBase> wrapper and add to list via reflection
            var _elementType = _list.GetType().GetGenericArguments()[0];
            var _ctor = _elementType.GetConstructor(new[] { typeof(object) });
            object _wrapper = null;
            if (_ctor != null)
            {
                // SerializableInterface<T> does not have ctor(object), so this path likely fails
                // Fallback to find ctor(TInterface)
                _ctor = null;
            }

            if (_ctor == null)
            {
                // Find any ctor with 1 parameter and try invoke with child
                var _ctors = _elementType.GetConstructors();
                foreach (var _c in _ctors)
                {
                    var _pars = _c.GetParameters();
                    if (_pars.Length == 1 && _pars[0].ParameterType.IsAssignableFrom(typeof(IStatBase)))
                    {
                        _ctor = _c;
                        break;
                    }
                }

                if (_ctor == null)
                {
                    // Handle generic parameter specifically: SerializableInterface<IStatBase>
                    var _ct = _elementType.GetConstructor(new[] { typeof(IStatBase) });
                    if (_ct != null) _ctor = _ct;
                }
            }

            if (_ctor == null)
            {
                // Last resort: invoke default and try set Value property
                _wrapper = Activator.CreateInstance(_elementType);
                var _valueProp = _elementType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                _valueProp?.SetValue(_wrapper, child);
            }
            else
            {
                _wrapper = _ctor.Invoke(new object[] { child });
            }

            var _addMethod = _list.GetType().GetMethod("Add");
            _addMethod?.Invoke(_list, new[] { _wrapper });
        }

        private static System.Collections.IList ensure_children_list(IStatBase parent)
        {
            var _type = parent.GetType();
            Type _t = _type;
            while (_t != null)
            {
                if (_t.IsGenericType && _t.GetGenericTypeDefinition().Name.StartsWith("WhiteBoard"))
                {
                    var _childrenField = _t.GetField("Children",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_childrenField != null)
                    {
                        var _current = _childrenField.GetValue(parent);
                        if (_current == null)
                        {
                            // Instantiate list with proper generic argument from field type
                            var _fieldType =
                                _childrenField.FieldType; // should be List<SerializableInterface<IStatBase>>
                            var _instance = Activator.CreateInstance(_fieldType);
                            _childrenField.SetValue(parent, _instance);
                            return _instance as System.Collections.IList;
                        }

                        return _current as System.Collections.IList;
                    }
                }

                _t = _t.BaseType;
            }

            return null;
        }

        private static object get_fallback_value(IEntity entity, string fallbackFieldName, Type expectedType)
        {
            try
            {
                var _entityType = entity.GetType();

                // Try to find field
                var _field = _entityType.GetField(fallbackFieldName, BINDING_FLAGS);
                if (_field != null && expectedType.IsAssignableFrom(_field.FieldType))
                {
                    return _field.GetValue(entity);
                }

                // Try to find property
                var _property = _entityType.GetProperty(fallbackFieldName, BINDING_FLAGS);
                if (_property != null && expectedType.IsAssignableFrom(_property.PropertyType) && _property.CanRead)
                {
                    return _property.GetValue(entity);
                }

                Debug.LogWarning(
                    $"Fallback field/property '{fallbackFieldName}' not found or has wrong type on {entity.Name}");
                return null;
            }
            catch (Exception _e)
            {
                Debug.LogWarning($"Exception getting fallback value from '{fallbackFieldName}': {_e.Message}");
                return null;
            }
        }
    }
}