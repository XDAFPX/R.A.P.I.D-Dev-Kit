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

    public interface ISaveData
    {
        Dictionary<string, object> Data { get; }
    }

    public struct GenericSaveData : ISaveData
    {
        public GenericSaveData(Dictionary<string, object> data)
        {
            Data = data;
        }

        public Dictionary<string, object> Data { get; } 
    }

}