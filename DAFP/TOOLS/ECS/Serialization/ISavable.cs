using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISavable
    {
        public ISaveData Save();
        public void Load(ISaveData saveData);
    }

    public  sealed class  GenericSaveData : ISaveData
    {
        private readonly Dictionary<string, object> _data;

        public GenericSaveData(Dictionary<string, object> data) => _data = data;

        public bool TryGet(string key, out object value) => _data.TryGetValue(key, out value);
        public void Set(string key, object value) => _data[key] = value;
        public IEnumerable<string> Keys => _data.Keys;
    }

}