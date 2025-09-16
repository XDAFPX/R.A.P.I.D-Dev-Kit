using System;
using System.Collections.Generic;
using System.IO;
using DAFP.TOOLS.Common.Utill;
using Newtonsoft.Json;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class SaveSerializationService : ISerializationService
    {
        public string GetFullSavePath(string domainName, string stateName, string name)
        {
            // Step 1: Construct relative save path info
            var pathInfo = ISerializationService.ConstructSavePath(domainName, stateName, name);

            // Step 2: Build and return the full save path under persistentDataPath
            return Path.Combine(Application.persistentDataPath, pathInfo.FolderPath, pathInfo.FileName);
        }

        public void DeleteSave(string domainName, string stateName, string name)
        {
            // Step 1: Construct relative paths
            var pathInfo = ISerializationService.ConstructSavePath(domainName, stateName, name);

            // Step 2: Build full file path
            var fullPath = Path.Combine(Application.persistentDataPath, pathInfo.FolderPath, pathInfo.FileName);

            // Step 3: Delete file if it exists
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Debug.Log($"Deleted save file at: {fullPath}");

                // Step 4: Clean up empty folder
                var folderPath = Path.Combine(Application.persistentDataPath, pathInfo.FolderPath);
                if (Directory.Exists(folderPath) && Directory.GetFileSystemEntries(folderPath).Length == 0)
                {
                    Directory.Delete(folderPath);
                    Debug.Log($"Deleted empty folder at: {folderPath}");
                }
            }
            else
            {
                // Step 5: Warn if not found
                Debug.LogWarning($"Save file not found at: {fullPath}, nothing to delete.");
            }
        }

        public bool SaveExist(string domainName, string stateName, string name)
        {
            var pathInfo = ISerializationService.ConstructSavePath(domainName, stateName, name);
            var fullPath = Path.Combine(Application.persistentDataPath, pathInfo.FolderPath, pathInfo.FileName);
            return File.Exists(fullPath);
        }

        public void Save(Dictionary<string, object> data, string domainName, string stateName, string name)
        {
            // 1. Construct relative paths
            var pathInfo = ISerializationService.ConstructSavePath(domainName, stateName, name);

            // 2. Build full folder path under persistentDataPath
            var folderPath = Path.Combine(Application.persistentDataPath, pathInfo.FolderPath);
            Directory.CreateDirectory(folderPath);

            // 3. Build full file path
            var fullPath = Path.Combine(folderPath, pathInfo.FileName);

            // 4. Serialize and write to file
            var json = JsonConvert.SerializeObject(
                data,
                Formatting.Indented,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
            );
            File.WriteAllText(fullPath, json);
            Debug.Log($"Saved save to : {fullPath}");
        }

        public Dictionary<string, object> Load(string domainName, string stateName, string name)
        {
            // 1. Construct relative paths
            var pathInfo = ISerializationService.ConstructSavePath(domainName, stateName, name);

            // 2. Build full file path under persistentDataPath
            var fullPath = Path.Combine(Application.persistentDataPath, pathInfo.FolderPath, pathInfo.FileName);

            // 3. Ensure file exists
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Save file not found at {fullPath}");
                return new Dictionary<string, object>();
            }

            // 4. Read and deserialize
            var json = File.ReadAllText(fullPath);

            Debug.Log($"Loaded save from : {fullPath}");
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(
                json,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
            );
        }
    }
}