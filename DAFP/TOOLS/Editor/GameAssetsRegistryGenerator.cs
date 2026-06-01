using UnityEngine;

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
            EditorApplication.delayCall -= generate_prefab_address_registry;
            EditorApplication.delayCall += generate_prefab_address_registry;
        }

        public static string GENERATED_CLASS_NAME = nameof(GameAssetsRegistryGenerator).Replace("Generator", "");

        private static void generate_prefab_address_registry()
        {
            // // Locate this script to decide where to place the generated file (same logic as TagGenerator)
            // string[] _guids = AssetDatabase.FindAssets($"t:Script {nameof(GameAssetsRegistryGenerator)}");
            // if (_guids == null || _guids.Length == 0) return;
            // string _path = AssetDatabase.GUIDToAssetPath(_guids[0]);
            // string _folder = Path.GetDirectoryName(_path);
            // if (Path.GetFileName(_folder).Equals("Editor", System.StringComparison.OrdinalIgnoreCase))
            // {
            //     _folder = Path.GetDirectoryName(_folder);
            // }
            //
            // // Get Addressables settings via reflection to avoid hard dependency on Addressables editor assembly
            // object _settings = get_addressable_settings();
            // if (_settings == null)
            // {
            //     write_registry(_folder, new List<string>(), new Dictionary<string, List<string>>());
            //     return;
            // }
            //
            // var _addresses = new List<string>(128);
            // var _byPrefix = new Dictionary<string, List<string>>();
            //
            // IEnumerable _groups = get_property<IEnumerable>(_settings, "groups");
            // if (_groups != null)
            // {
            //     foreach (var _group in _groups)
            //     {
            //         if (_group == null) continue;
            //         IEnumerable _entries = get_property<IEnumerable>(_group, "entries");
            //         if (_entries == null) continue;
            //
            //         foreach (var _e in _entries)
            //         {
            //             if (_e == null) continue;
            //             string _assetPath = get_string_property(_e, "AssetPath", "assetPath");
            //             if (string.IsNullOrEmpty(_assetPath) || !_assetPath.EndsWith(".prefab")) continue;
            //
            //             string _address = get_string_property(_e, "address", "Address");
            //             if (string.IsNullOrEmpty(_address)) continue;
            //
            //             if (!_address.StartsWith("GAME.Assets."))
            //                 continue;
            //
            //             _addresses.Add(_address);
            //
            //             var _parts = _address.Split('.');
            //             if (_parts.Length >= 4 && _parts[0] == "GAME" && _parts[1] == "Assets")
            //             {
            //                 string _prefix = _parts[2];
            //                 string _uName = string.Join(".", _parts.Skip(3));
            //
            //                 if (!_byPrefix.TryGetValue(_prefix, out var _list))
            //                 {
            //                     _list = new List<string>();
            //                     _byPrefix[_prefix] = _list;
            //                 }
            //
            //                 _list.Add(_uName);
            //             }
            //         }
            //     }
            // }
            //
            // _addresses.Sort(System.StringComparer.Ordinal);
            // foreach (var _kv in _byPrefix.ToList())
            //     _kv.Value.Sort(System.StringComparer.Ordinal);
            //
            // write_registry(_folder, _addresses, _byPrefix);
        }

        private static void write_registry(string folder,
            List<string> addresses, //--TODO probably also include a bunch of const GameAssetInfo
            Dictionary<string, List<string>> byPrefix)
        {
            string _outputPath = Path.Combine(folder, $"{GENERATED_CLASS_NAME}.cs");

            string _allAddresses = string.Join(", ", addresses.Select(a => $"\"{a}\""));

            string _code = $@"public static class {GENERATED_CLASS_NAME}
{{
    public static readonly string[] AllAddresses = new string[] {{ {_allAddresses} }};
}}
";

            Directory.CreateDirectory(folder);
            if (File.Exists(_outputPath))
            {
                var _existing = File.ReadAllText(_outputPath);
                if (string.Equals(_existing.Trim(), _code.Trim(), StringComparison.Ordinal))
                    return; // nothing changed, don't write, don't refresh, don't recompile
            }

            File.WriteAllText(_outputPath, _code);
            AssetDatabase.Refresh();
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
            var _prop = _type.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (_prop != null)
            {
                object _val = _prop.GetValue(obj, null);
                if (_val is T _tv) return _tv;
            }

            var _field = _type.GetField(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
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
                catch
                {
                }
            }

            return null;
        }
    }
#endif
}