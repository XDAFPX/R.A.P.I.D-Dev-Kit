namespace DAFP.TOOLS.Common.Utill
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class ResourceSaver
    {
        /// <summary>
        /// Saves a string as a .txt under Assets/Resources/{relativeFolder}/,
        /// creating all necessary sub‐directories automatically.
        /// Example: SaveTextToResources("MyData/SubFolder", "LevelInfo", "Hello");
        /// </summary>
        /// <param name="relativeFolder">Folder path under Resources (e.g. "MyData/Sub/Sub2")</param>
        /// <param name="fileName">Name of the file without extension</param>
        /// <param name="content">Text content to write</param>
        public static void SaveTextToResources(string relativeFolder, string fileName, string content)
        {
            // Build the absolute path to Assets/Resources/{relativeFolder}
            var resourcesPath = Path.Combine(Application.dataPath, "Resources");
            var folderPath = Path.Combine(resourcesPath, relativeFolder);

            // Create all directories in the path if they don't exist
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Write the file (adds .txt extension)
            var fullPath = Path.Combine(folderPath, fileName );
            File.WriteAllText(fullPath, content);

            // Refresh Unity's AssetDatabase so it shows up immediately
            AssetDatabase.Refresh();

            Debug.Log($"[ResourceSaver] Saved to {fullPath}");
        }
    }
}