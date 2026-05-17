using System.Collections.Generic;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;

namespace DAFP.TOOLS.BTs
{
    public interface IBlackBoard : ISavable
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
        bool Has(string key);
    }

    public class BlackBoard : IBlackBoard
    {
        private Dictionary<string, object> data = new();


        public BlackBoard(IEntity self)
        {
            data.Clear();
            data.Add("Self", self);
        }

        public BlackBoard(IEntity self,Dictionary<string,object> @new)
        {
            data.Clear();
            data = @new;
            data["Self"] = self;
            
        }

        public void Delete(string key)
        {
            data.Remove(key);
        }

        public void Set<T>(string key, T value)
        {
            data[key] = value;
        }

        public T Get<T>(string key)
        {
            if (key == default)
                return default;
            if (!Has(key)) return default;
            if (data.TryGetValue(key, out var _value) && _value is T _castValue) return _castValue;

            return default;
        }

        public bool Has(string key)
        {
            if (key == default)
                return default;
            return data.ContainsKey(key);
        }

        internal IEntity GetSelf()
        {
            return Get<IEntity>("Self");
        }

        public Dictionary<string, object> GetFullData()
        {
            return data;
        }

        public ISaveData Save()
        {
            return new GenericSaveData(data);
        }

        public void Load(ISaveData saveData)
        {
            data = saveData.Data;
        }
    }
}