using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DAFP.TOOLS.Common.Utill;
using ModestTree;
using NUnit.Framework;
using PixelRouge.Inspector.Extensions;
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
        public static void InjectStats(IEntity entity, object[] additionalSources)
        {
            if (entity?.Stats == null)
                return;

            if (extract_injectable_fields(entity, additionalSources, out var _refined, out var _fieldSources)) return;

            foreach (var _valueTuple in _refined)
            {
                var _source = (_fieldSources != null && _fieldSources.TryGetValue(_valueTuple.Item1, out var __s))
                    ? __s
                    : (object)entity;
                inject_stat(_valueTuple, entity, _source);
            }
        }


        public static void FixStats(IEntity entity, object[] additionalSources)
        {
            if (entity?.Stats == null)
                return;
            if (extract_declared_stats(entity, additionalSources, out var _refined)) return;


            // uint _maxDeph = _refined.MaxBy(t => t.Item2.DephRating());

            apply_flat_fix();


            foreach (var _valueTuple in _refined)
            {
                var _field = _valueTuple.Item1;
                var _naming = $"[{_valueTuple.Item2.StatName}]/({_valueTuple.Item1.Name})";


                if (DEBUG)
                    Debug.Log($"StatInjector: trying to fix  {_naming} :: ");

                PathBuilder _path = null;
                try
                {
                    _path = new PathBuilder(_valueTuple.Item2.StatName);
                }
                catch (Exception _e)
                {
                    if (DEBUG)
                        Debug.LogError($"StatInjector: failed to build path from {_naming} :: {_e}");
                    return;
                }

                if (entity.Stats.Has(_path))
                    continue;


                if (_path.GetTotalDepth() == 0)
                {
                    simple_resolve_stat(entity, _valueTuple, _naming);
                    continue;
                }

                var _data = _path.IterateProgressivePaths().ToArray();
                var _dataOnlyNames = _path.IterateRootToLeaf().ToArray();


                resolve_stat_dependencies(_data, _dataOnlyNames, _field, _valueTuple);


                var _leafStat = try_generate_stat(_valueTuple);
                entity.Stats.Add(_path, _leafStat);
            }


            void apply_flat_fix()
            {
                foreach (var _valueTuple in _refined.Where(((tuple) =>
                             new PathBuilder(tuple.Item2.StatName).GetTotalDepth() == 0)))
                {
                    var _naming = $"[{_valueTuple.Item2.StatName}]/({_valueTuple.Item1.Name})";
                    simple_resolve_stat(entity, _valueTuple, _naming);
                }
            }

            void resolve_stat_dependencies(string[] _data, string[] _dataOnlyNames, FieldInfo _field,
                (FieldInfo, DeclareStatAttribute) _valueTuple)
            {
                for (int _i = 0; _i < _data.Length - 1; _i++)
                {
                    var _entryPath = _data[_i];
                    var _name = _dataOnlyNames[_i];

                    if (entity.Stats.Has(new PathBuilder(_entryPath)))
                        continue;


                    var _foundMatchInDecleared = _refined.FirstOrDefault((tuple => tuple.Item2.StatName == _entryPath));
                    if (!_foundMatchInDecleared.Equals(default))
                    {
                        var _stat = try_generate_stat(_foundMatchInDecleared);

                        entity.Stats.Add(new PathBuilder(_entryPath), _stat);
                        continue;
                    }

                    var _auto = (_field, new DeclareStatAttribute(_entryPath, _valueTuple.Item2.FallbackObject));
                    var _autoStat = try_generate_stat(_auto);
                    entity.Stats.Add(new PathBuilder(_entryPath), _autoStat);
                }
            }
        }

        private static bool extract_declared_stats(IEntity entity, object[] additionalSources,
            out (FieldInfo, DeclareStatAttribute)[] _refined)
        {
            // Backward-compatible overload: ignore sources mapping
            return extract_declared_stats(entity, additionalSources, out _refined, out _);
        }

        // Collect declared fields only (for FixStats)
        private static bool extract_declared_stats(IEntity entity, object[] additionalSources,
            out (FieldInfo, DeclareStatAttribute)[] _refined,
            out Dictionary<FieldInfo, object> fieldSources)
        {
            object[] _sources = additionalSources.Concat(new[] { entity }).ToArray();

            var _rawFields = new List<FieldInfo>();
            fieldSources = new Dictionary<FieldInfo, object>();

            foreach (var _source in _sources)
            {
                var _fields = _source.GetAllFields(info => info.HasAttribute(typeof(DeclareStatAttribute)));

                foreach (var _f in _fields)
                {
                    if (!_rawFields.Contains(_f))
                        _rawFields.Add(_f);
                    if (!fieldSources.ContainsKey(_f))
                        fieldSources[_f] = _source;
                }
            }

            if (_rawFields.Count == 0)
            {
                _refined = null;
                fieldSources = null;
                return true;
            }

            _refined = _rawFields
                .Select(info => (info, info.GetCustomAttribute<DeclareStatAttribute>()))
                .ToArray();
            return false;
        }

        // Collect both declared and injection-only fields for injection pass
        private static bool extract_injectable_fields(IEntity entity, object[] additionalSources,
            out (FieldInfo field, string statName)[] _refined,
            out Dictionary<FieldInfo, object> fieldSources)
        {
            object[] _sources = additionalSources.Concat(new[] { entity }).ToArray();
            var _rawFields = new List<FieldInfo>();
            fieldSources = new Dictionary<FieldInfo, object>();

            foreach (var _source in _sources)
            {
                var _fields = _source.GetAllFields(info =>
                    info.HasAttribute(typeof(DeclareStatAttribute)) || info.HasAttribute(typeof(InjectStatAttribute)));

                foreach (var _f in _fields)
                {
                    if (!_rawFields.Contains(_f))
                        _rawFields.Add(_f);
                    if (!fieldSources.ContainsKey(_f))
                        fieldSources[_f] = _source;
                }
            }

            if (_rawFields.Count == 0)
            {
                _refined = null;
                fieldSources = null;
                return true;
            }

            _refined = _rawFields
                .Select(info =>
                {
                    var decl = info.GetCustomAttribute<DeclareStatAttribute>();
                    if (decl != null) return (info, decl.StatName);
                    var inj = info.GetCustomAttribute<InjectStatAttribute>();
                    return (info, inj.StatName);
                })
                .ToArray();
            return false;
        }

        private static void inject_stat((FieldInfo field, string statName) field, IEntity entity, object source)
        {
            var path = new PathBuilder(field.statName);
            if (entity.Stats.Has(path, out var _statBase))
            {
                field.field.SetValue(source, _statBase);
            }
            else
            {
                var _naming = $"[{field.statName}]/({field.field.Name})";
                Debug.LogError($"StatInjector: failed to inject:  {_naming} ::");
                Debug.LogError($"StatInjector: critical error");
            }
        }

        private static IStatBase try_generate_stat((FieldInfo, DeclareStatAttribute) _foundMatchInDecleared)
        {
            IStatBase stat = null;
            try
            {
                stat = generate_stat_from_info(_foundMatchInDecleared);
            }
            catch (Exception _e)
            {
                if (DEBUG)
                    Debug.LogError(
                        $"StatInjector: failed to generate stat from info {_foundMatchInDecleared.Item2.StatName} :: {_e}");
            }

            return stat;
        }

        private static void simple_resolve_stat(IEntity entity, (FieldInfo, DeclareStatAttribute) valueTuple,
            string naming)
        {
            if (check_field(valueTuple, entity.Stats))
            {
                try
                {
                    entity.Stats.Add(generate_stat_from_info(valueTuple));
                }
                catch (Exception _e)
                {
                    if (DEBUG)
                        Debug.LogError($"StatInjector: failed to generate stat from info {naming} :: {_e}");
                }
            }
        }

        public static bool DEBUG = true;

        private static bool check_field((FieldInfo, DeclareStatAttribute) field, IStatContainer stats)
        {
            PathBuilder _path = null;
            var _naming = $"[{field.Item2.StatName}]/({field.Item1.Name})";
            try
            {
                _path = new PathBuilder(field.Item2.StatName);
            }
            catch (Exception _ex)
            {
                if (DEBUG)
                    Debug.Log($"StatInjector: field resolution failed {_naming} :: " + _ex.Message);
                return false;
            }

            if (stats.Has(_path))
                return false;


            return true;
        }

        private static IStatBase generate_stat_from_info((FieldInfo, DeclareStatAttribute) field)
        {
            PathBuilder _path = null;
            try
            {
                _path = new PathBuilder(field.Item2.StatName);
            }
            catch (Exception _e)
            {
                Debug.LogError($"StatInjector: stat {field.Item1.Name} has invalid name:: {_e} ");
                throw;
            }

            var _naming = $"[{field.Item2.StatName}]/({field.Item1.Name})";
            if (DEBUG)
                Debug.Log($"StatInjector: stat {_naming} :: ");

            if (field.Item1.FieldType is { IsGenericType: true })
            {
                // Choose a concrete whiteboard implementation based on the generic type argument.
                // This makes it easy to extend with specialized boards later (e.g., IntBoard, FloatBoard, etc.).
                Type _gen = field.Item1.FieldType.GenericTypeArguments[0];

                Type boardType;
                switch (Type.GetTypeCode(_gen))
                {
                    case TypeCode.Boolean:
                        boardType = typeof(DAFP.TOOLS.ECS.BigData.Common.BoolBoard);
                        break;
                    default:
                        boardType = typeof(GenericWhiteBoard<>).MakeGenericType(_gen);
                        break;
                }

                var _inst = (IStatBase)Activator.CreateInstance(boardType);
                configure_stat_defaults(field, _inst, _path, _gen);

                return _inst;
            }

            IStatBase _stat = (IStatBase)Activator.CreateInstance(field.Item1.FieldType);
            configure_stat_defaults(field, _stat, _path, null);
            return _stat;
        }

        private static void configure_stat_defaults((FieldInfo, DeclareStatAttribute) field, IStatBase inst,
            PathBuilder path,
            Type genericsType)
        {
            inst.Name = path.GetLeaf();

            object _fallbackValue = field.Item2.FallbackObject;


            if (genericsType == null)
            {
                if (_fallbackValue == default) // fix for placeholders
                    return;

                inst.SetAbsoluteDefault(_fallbackValue);
                return;
            }

            // Apply Max/Default as before (convert if needed)
            object _valueToApply;
            if (_fallbackValue == null || genericsType.IsAssignableFrom(_fallbackValue.GetType()))
            {
                _valueToApply = _fallbackValue;
                inst.SetAbsoluteDefault(_valueToApply);
            }
            else
            {
                object _convertedValue = try_convert(_fallbackValue, genericsType);
                _valueToApply = _convertedValue;
                inst.SetAbsoluteDefault(_valueToApply);
            }
        }


        public static object try_convert(object value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            try
            {
                // Try IConvertible (handles primitives, strings, etc.)
                if (value is IConvertible)
                {
                    return Convert.ChangeType(value, targetType);
                }

                // Try implicit/explicit operators
                var _convertMethod = targetType.GetMethod("op_Implicit", new[] { value.GetType() })
                                     ?? targetType.GetMethod("op_Explicit", new[] { value.GetType() });
                if (_convertMethod != null)
                {
                    return _convertMethod.Invoke(null, new[] { value });
                }

                // Try TypeConverter
                var _converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                if (_converter.CanConvertFrom(value.GetType()))
                {
                    return _converter.ConvertFrom(value);
                }

                // Try constructor that takes the source type
                var _constructor = targetType.GetConstructor(new[] { value.GetType() });
                if (_constructor != null)
                {
                    return _constructor.Invoke(new[] { value });
                }

                // Last resort: try parsing if target type has Parse method and value is string
                if (value is string _str)
                {
                    var _parseMethod = targetType.GetMethod("Parse", new[] { typeof(string) });
                    if (_parseMethod != null && _parseMethod.IsStatic)
                    {
                        return _parseMethod.Invoke(null, new object[] { _str });
                    }
                }
            }
            catch
            {
                // If all conversions fail, return default value
            }

            // Return default value for the type
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        // Utility: Compare two objects as the specified generic type using Comparer<T>.Default
        private static int CompareAs(object a, object b, Type gen)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            var comparerType = typeof(Comparer<>).MakeGenericType(gen);
            var defaultProp = comparerType.GetProperty("Default");
            var defaultComparer = defaultProp.GetValue(null);
            var compareMethod = comparerType.GetMethod("Compare", new[] { gen, gen });
            return (int)compareMethod.Invoke(defaultComparer, new[] { a, b });
        }

        public class PathBuilder
        {
            private List<string> segments;
            private int currentIndex;
            private static readonly char[] SEPARATORS = { '/', '\\', '.' };

            public PathBuilder(string path, char[] separators = null)
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentException("Path cannot be null or empty", nameof(path));

                var _separators = separators ?? SEPARATORS;
                segments = path.Split(_separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (segments.Count == 0)
                    throw new ArgumentException("Path must contain at least one segment", nameof(path));

                currentIndex = segments.Count - 1;
            }

            public string GetRoot() => segments[0];
            public string GetLeaf() => segments[^1];
            public string GetCurrent() => segments[currentIndex];

            public PathBuilder MoveUp(int steps = 1)
            {
                currentIndex = Math.Max(0, currentIndex - steps);
                return this;
            }

            public PathBuilder MoveDown(int steps = 1)
            {
                currentIndex = Math.Min(segments.Count - 1, currentIndex + steps);
                return this;
            }

            public PathBuilder MoveToRoot()
            {
                currentIndex = 0;
                return this;
            }

            public PathBuilder MoveToLeaf()
            {
                currentIndex = segments.Count - 1;
                return this;
            }

            public int GetDepth() => currentIndex;
            public int GetTotalDepth() => segments.Count - 1;

            public string GetPathToCurrent() => string.Join(SEPARATORS[0], segments.Take(currentIndex + 1));
            public string GetFullPath() => string.Join(SEPARATORS[0], segments);

            public string GetParent() => currentIndex > 0 ? segments[currentIndex - 1] : null;
            public string GetChild() => currentIndex < segments.Count - 1 ? segments[currentIndex + 1] : null;

            public List<string> GetAncestors() => segments.Take(currentIndex).Reverse().ToList();
            public List<string> GetDescendants() => segments.Skip(currentIndex + 1).ToList();

            public IEnumerable<string> IterateProgressivePaths()
            {
                for (int _i = 0; _i < segments.Count; _i++)
                    yield return string.Join(SEPARATORS[0], segments.Take(_i + 1));
            }

            public IEnumerable<string> IterateRootToLeaf()
            {
                foreach (var _s in segments) yield return _s;
            }

            public IEnumerable<string> IterateLeafToRoot()
            {
                for (int _i = segments.Count - 1; _i >= 0; _i--)
                    yield return segments[_i];
            }

            public IEnumerable<string> IterateToRoot()
            {
                for (int _i = currentIndex; _i >= 0; _i--)
                    yield return segments[_i];
            }

            public IEnumerable<string> IterateToLeaf()
            {
                for (int _i = currentIndex; _i < segments.Count; _i++)
                    yield return segments[_i];
            }

            public IEnumerable<(string segment, int index, int depth)> IterateWithInfo()
            {
                for (int _i = 0; _i < segments.Count; _i++)
                    yield return (segments[_i], _i, _i);
            }

            public void ForEach(Action<string> action) => segments.ForEach(action);

            public void ForEach(Action<string, int, int> action)
            {
                for (int _i = 0; _i < segments.Count; _i++)
                    action(segments[_i], _i, _i);
            }

            public override string ToString() => GetPathToCurrent();
        }
    }
}