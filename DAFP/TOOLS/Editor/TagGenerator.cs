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

            string code = $@"
public static class TagRegistry
{{
    public static readonly string[] AllTags = new string[] {{ {tagsArray} }};
}}
";

            Directory.CreateDirectory(folder);
            File.WriteAllText(outputPath, code);
            AssetDatabase.Refresh();
            Debug.Log("TagRegistry updated at: " + outputPath);
        }
    }
#endif
}