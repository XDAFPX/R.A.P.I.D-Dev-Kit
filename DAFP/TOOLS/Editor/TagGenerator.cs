namespace RapidLib.DAFP.TOOLS.Editor
{
#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;
    using System.IO;

    [InitializeOnLoad]
    public static class TagGenerator
    {
        static TagGenerator()
        {
            EditorApplication.delayCall += GenerateTagRegistry;
        }

        private static void GenerateTagRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:Script TagGenerator");
            if (guids.Length == 0) return;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            string folder = Path.GetDirectoryName(path);

            if (Path.GetFileName(folder).Equals("Editor", System.StringComparison.OrdinalIgnoreCase))
            {
                folder = Path.GetDirectoryName(folder);
            }

            string outputPath = Path.Combine(folder, "TagRegistry.cs");

            // Генеруємо масив тегів
            string[] tags = InternalEditorUtility.tags;
            string tagsArray = string.Join(", ", System.Array.ConvertAll(tags, t => $"\"{t}\""));

            // Local sanitizer to build valid enum identifiers
            string SanitizeIdentifier(string input)
            {
                if (string.IsNullOrEmpty(input)) return "_";
                var chars = input.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];
                    if (!(char.IsLetterOrDigit(c) || c == '_'))
                    {
                        chars[i] = '_';
                    }
                }
                string result = new string(chars);
                if (!(char.IsLetter(result[0]) || result[0] == '_'))
                    result = "_" + result;
                return result;
            }

            // Build enum entries with sanitized and unique identifiers matching AllTags order
            System.Collections.Generic.HashSet<string> used = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            System.Text.StringBuilder enumSb = new System.Text.StringBuilder();
            for (int i = 0; i < tags.Length; i++)
            {
                string id = SanitizeIdentifier(tags[i]);
                string baseId = id;
                int suffix = 1;
                while (!used.Add(id))
                {
                    id = baseId + "_" + suffix++;
                }
                enumSb.Append("        ").Append(id);
                if (i < tags.Length - 1) enumSb.Append(',');
                enumSb.AppendLine();
            }

            string code = $@"
public static class TagRegistry
{{
    public static readonly string[] AllTags = new string[] {{ {tagsArray} }};

    public enum Tag
    {{
{enumSb}    }}

    public static string GetTagString(Tag t) => AllTags[(int)t];
}}
";

            Directory.CreateDirectory(folder);
            bool shouldWrite = true;
            if (File.Exists(outputPath))
            {
                var existing = File.ReadAllText(outputPath);
                if (string.Equals(existing, code, System.StringComparison.Ordinal))
                {
                    shouldWrite = false;
                }
            }

            if (shouldWrite)
            {
                File.WriteAllText(outputPath, code);
                AssetDatabase.Refresh();
                Debug.Log("[TagRegistry] updated at: " + outputPath);
            }
            else
            {
                // No changes detected; skip regeneration
            }
        }
    }
#endif
}