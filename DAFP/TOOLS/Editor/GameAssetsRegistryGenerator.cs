namespace RapidLib.DAFP.TOOLS.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;

    [InitializeOnLoad]
    public static class GameAssetsRegistryGenerator
    {
        static GameAssetsRegistryGenerator()
        {
            EditorApplication.delayCall += generate_prefab_address_registry;
        }

        public static string GENERATED_CLASS_NAME = nameof(GameAssetsRegistryGenerator).Replace("Generator","");
        private static void generate_prefab_address_registry()
        {
            // Locate this script to decide where to place the generated file (same logic as TagGenerator)
            string[] _guids = AssetDatabase.FindAssets($"t:Script {nameof(GameAssetsRegistryGenerator)}");
            if (_guids == null || _guids.Length == 0) return;
            string _path = AssetDatabase.GUIDToAssetPath(_guids[0]);
            string _folder = Path.GetDirectoryName(_path);
            if (Path.GetFileName(_folder).Equals("Editor", System.StringComparison.OrdinalIgnoreCase))
            {
                _folder = Path.GetDirectoryName(_folder);
            }

            // Get Addressables settings via reflection to avoid hard dependency on Addressables editor assembly
            object _settings = get_addressable_settings();
            if (_settings == null)
            {
                write_registry(_folder, new List<string>(), new Dictionary<string, List<string>>());
                return;
            }

            var _addresses = new List<string>(128);
            var _byPrefix = new Dictionary<string, List<string>>();

            IEnumerable _groups = get_property<IEnumerable>(_settings, "groups");
            if (_groups != null)
            {
                foreach (var _group in _groups)
                {
                    if (_group == null) continue;
                    IEnumerable _entries = get_property<IEnumerable>(_group, "entries");
                    if (_entries == null) continue;

                    foreach (var _e in _entries)
                    {
                        if (_e == null) continue;
                        string _assetPath = get_string_property(_e, "AssetPath", "assetPath");
                        if (string.IsNullOrEmpty(_assetPath) || !_assetPath.EndsWith(".prefab")) continue;

                        string _address = get_string_property(_e, "address", "Address");
                        if (string.IsNullOrEmpty(_address)) continue;

                        if (!_address.StartsWith("GAME.Assets."))
                            continue;

                        _addresses.Add(_address);

                        var _parts = _address.Split('.');
                        if (_parts.Length >= 4 && _parts[0] == "GAME" && _parts[1] == "Assets")
                        {
                            string _prefix = _parts[2];
                            string _uName = string.Join(".", _parts.Skip(3));

                            if (!_byPrefix.TryGetValue(_prefix, out var _list))
                            {
                                _list = new List<string>();
                                _byPrefix[_prefix] = _list;
                            }
                            _list.Add(_uName);
                        }
                    }
                }
            }

            _addresses.Sort(System.StringComparer.Ordinal);
            foreach (var _kv in _byPrefix.ToList())
                _kv.Value.Sort(System.StringComparer.Ordinal);

            write_registry(_folder, _addresses, _byPrefix);
        }

        private static void write_registry(string folder, List<string> addresses, Dictionary<string, List<string>> byPrefix)
        {
            string _outputPath = Path.Combine(folder, $"{GENERATED_CLASS_NAME}.cs");

            // Prepare code
            string _allAddresses = string.Join(", ", addresses.Select(a => $"\"{a}\""));

            // Order prefixes deterministically
            var _ordered = byPrefix.OrderBy(k => k.Key, System.StringComparer.Ordinal).ToList();
            // Build Prefix enum and mapping array
            var _prefixEnumSb = new System.Text.StringBuilder();
            var _prefixStringsSb = new System.Text.StringBuilder();
            var _usedPrefixIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            for (int _i = 0; _i < _ordered.Count; _i++)
            {
                var _key = _ordered[_i].Key;
                string _id = sanitize_identifier(_key);
                string _baseId = _id;
                int _suffix = 1;
                while (!_usedPrefixIds.Add(_id))
                {
                    _id = _baseId + "_" + _suffix++;
                }
                _prefixEnumSb.Append("        ").Append(_id);
                if (_i < _ordered.Count - 1) _prefixEnumSb.Append(',');
                _prefixEnumSb.AppendLine();
                _prefixStringsSb.Append('"').Append(_key).Append('"');
                if (_i < _ordered.Count - 1) _prefixStringsSb.Append(", ");
            }

            // Generate per-prefix classes with UName enums and helpers (no ByPrefix wrapper)
            var _byPrefixCode = new System.Text.StringBuilder();
            foreach (var _kv in _ordered)
            {
                string _classId = sanitize_identifier(_kv.Key);
                string[] _unames = _kv.Value.ToArray();
                string _unamesArray = string.Join(", ", _unames.Select(u => $"\"{u}\""));
                string _addressesForPrefix = string.Join(", ", _unames.Select(u => $"\"GAME.Assets.{_kv.Key}.{u}\""));

                // Build enum for UNames with uniqueness after sanitization
                var _usedUIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
                var _uEnumSb = new System.Text.StringBuilder();
                for (int _i = 0; _i < _unames.Length; _i++)
                {
                    string _id = sanitize_identifier(_unames[_i]);
                    string _baseId = _id;
                    int _suffix = 1;
                    while (!_usedUIds.Add(_id))
                    {
                        _id = _baseId + "_" + _suffix++;
                    }
                    _uEnumSb.Append("            ").Append(_id);
                    if (_i < _unames.Length - 1) _uEnumSb.Append(',');
                    _uEnumSb.AppendLine();
                }

                _byPrefixCode.AppendLine($"        public static class {_classId}");
                _byPrefixCode.AppendLine("        {");
                _byPrefixCode.AppendLine($"            public const string Prefix = \"{_kv.Key}\";");
                _byPrefixCode.AppendLine($"            public static readonly string[] UNames = new string[] {{ {_unamesArray} }};");
                _byPrefixCode.AppendLine($"            public static readonly string[] Addresses = new string[] {{ {_addressesForPrefix} }};");
                _byPrefixCode.AppendLine($"            public enum N\n            {{\n{_uEnumSb}            }}");
                _byPrefixCode.AppendLine("            public static string GetUNameString(N u) => UNames[(int)u];");
                _byPrefixCode.AppendLine("            public static string GetAddress(N u) => Addresses[(int)u];");
                _byPrefixCode.AppendLine("        }");
            }

            string _code = $@"
public static class {GENERATED_CLASS_NAME}
{{
    public static readonly string[] AllAddresses = new string[] {{ {_allAddresses} }};

    public enum Prefix
    {{
{_prefixEnumSb}    }}
    private static readonly string[] PrefixStrings = new string[] {{ {_prefixStringsSb} }};
    public static string GetPrefixString(Prefix p) => PrefixStrings[(int)p];
{_byPrefixCode}
}}
";

            Directory.CreateDirectory(folder);
            bool _shouldWrite = true;
            if (File.Exists(_outputPath))
            {
                var _existing = File.ReadAllText(_outputPath);
                if (string.Equals(_existing, _code, System.StringComparison.Ordinal))
                {
                    _shouldWrite = false;
                }
            }

            if (_shouldWrite)
            {
                File.WriteAllText(_outputPath, _code);
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log($"[{GENERATED_CLASS_NAME}] updated at: " + _outputPath);
            }
            else
            {
                // No changes detected; skip regeneration
            }
        }

        private static string sanitize_identifier(string input)
        {
            if (string.IsNullOrEmpty(input)) return "_";
            var _chars = input.ToCharArray();
            for (int _i = 0; _i < _chars.Length; _i++)
            {
                char _c = _chars[_i];
                if (!(char.IsLetterOrDigit(_c) || _c == '_'))
                {
                    _chars[_i] = '_';
                }
            }
            string _result = new string(_chars);
            if (!char.IsLetter(_result[0]) && _result[0] != '_')
                _result = "_" + _result;
            return _result;
        }

        // Reflection helpers to decouple from Addressables editor assembly
        private static object get_addressable_settings()
        {
            // Try to locate UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings
            var _type = find_type("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
            if (_type == null)
                return null;
            var _prop = _type.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
            return _prop?.GetValue(null, null);
        }

        private static T get_property<T>(object obj, string propertyName)
        {
            if (obj == null) return default;
            var _type = obj.GetType();
            var _prop = _type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (_prop != null)
            {
                object _val = _prop.GetValue(obj, null);
                if (_val is T _tv) return _tv;
            }
            var _field = _type.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (_field != null)
            {
                object _val = _field.GetValue(obj);
                if (_val is T _tv) return _tv;
            }
            return default;
        }

        private static T get_property<T>(object obj, string propertyName1, string propertyName2)
        {
            var _v = get_property<T>(obj, propertyName1);
            if (!Equals(_v, default(T))) return _v;
            return get_property<T>(obj, propertyName2);
        }

        private static string get_string_property(object obj, string propertyName1, string propertyName2)
        {
            var _v1 = get_property<string>(obj, propertyName1);
            if (!string.IsNullOrEmpty(_v1)) return _v1;
            var _v2 = get_property<string>(obj, propertyName2);
            return _v2;
        }

        private static System.Type find_type(string fullName)
        {
            var _t = System.Type.GetType(fullName);
            if (_t != null) return _t;
            foreach (var _asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    _t = _asm.GetType(fullName);
                    if (_t != null) return _t;
                }
                catch { }
            }
            return null;
        }
    }
#endif
}
