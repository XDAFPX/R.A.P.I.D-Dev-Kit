using System;
using System.Collections.Generic;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISerializationService
    {
        public static (string FolderPath, string FileName) ConstructSavePath(
            string domainName,
            string stateName,
            string name)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("domainName cannot be null or empty.", nameof(domainName));
            if (string.IsNullOrEmpty(stateName))
                throw new ArgumentException("stateName cannot be null or empty.", nameof(stateName));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty.", nameof(name));

            var folder = @$"{domainName}\{stateName}\World\Ent";
            var file = $"{name}.json";
            return (folder, file);
        }

        public string GetFullSavePath(string domainName, string stateName, string name);
        public void DeleteSave(string domainName, string stateName, string name);
        public bool SaveExist(string domainName, string stateName, string name);
        public void Save(Dictionary<string, object> data, string domainName, string stateName, string name);
        public Dictionary<string, object> Load(string domainName, string stateName, string name);
    }
}