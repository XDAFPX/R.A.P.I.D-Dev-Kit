using System;
using System.Collections.Generic;
using System.IO;
using DAFP.TOOLS.Common.Utill;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class ConfigSerializationService : ISerializationService
    {
        public string GetFullSavePath(string domainName, string stateName, string name)
        {
            // 1. Build the relative save path
            var pathInfo = ISerializationService.ConstructSavePath(domainName, stateName, name);
            // 2. Compose the Resources folder on disk
            var resourcesDir = Path.Combine(Application.dataPath, "Resources", pathInfo.FolderPath);
            // 3. Return the full file path
            return Path.Combine(resourcesDir, pathInfo.FileName);
        }

        public void DeleteSave(string domainName, string stateName, string name)
        {
            // Build the relative save path
            var path = ISerializationService.ConstructSavePath(domainName, stateName, name);

            // Construct full path under Assets/Resources
            var resourcesDir = Path.Combine(Application.dataPath, "Resources", path.FolderPath);
            var filePath = Path.Combine(resourcesDir, path.FileName);

            // Delete the JSON file if it exists
            if (File.Exists(filePath)) File.Delete(filePath);

            // In the Editor, also delete the .meta file and refresh
#if UNITY_EDITOR
            var metaFile = filePath + ".meta";
            if (File.Exists(metaFile)) File.Delete(metaFile);

            AssetDatabase.Refresh();
#endif
        }

        public bool SaveExist(string domainName, string stateName, string name)
        {
            // 1. Build the relative resource path
            var path = ISerializationService.ConstructSavePath(domainName, stateName, name);
            // Remove ".json" extension and normalize separators
            var resourcePath = Path.Combine(path.FolderPath, Path.GetFileNameWithoutExtension(path.FileName))
                .Replace('\\', '/');
            // 2. Attempt to load the TextAsset
            var asset = Resources.Load<TextAsset>(resourcePath);
            // 3. Return true if the asset was found
            return asset != null;
        }

        public void Save(Dictionary<string, object> data, string domainName, string stateName, string name)
        {
            var path = ISerializationService.ConstructSavePath(domainName, stateName, name);
            ResourceSaver.SaveTextToResources(path.FolderPath, path.FileName,
                JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }

        public Dictionary<string, object> Load(string domainName, string stateName, string name)
        {
            var path = ISerializationService.ConstructSavePath(domainName, stateName, name);
            var asset =
                Resources.Load<TextAsset>((path.FolderPath + @"\" + path.FileName).Replace(@"\", "/")
                    .Replace(".json", string.Empty));

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(asset.text,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}