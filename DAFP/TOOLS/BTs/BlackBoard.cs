using System.Collections.Generic;
using System.Text;
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

        public BlackBoard(IEntity self, Dictionary<string, object> @new)
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
            return new GenericSaveData(new Dictionary<string, object>(data));
        }

        public void Load(ISaveData saveData)
        {
            data.Clear();
            foreach (var _saveDataKey in saveData.Keys)
            {
                if (saveData.TryGet(_saveDataKey, out var _value))
                {
                    data[_saveDataKey] = _value;
                }
            }
        }

        public override string ToString()
        {
            if (data == null || data.Count == 0)
                return "BlackBoard { Empty }";

            var sb = new StringBuilder();
            sb.AppendLine("BlackBoard {");

            foreach (var kvp in data)
            {
                string valueStr = kvp.Value != null ? kvp.Value.ToString() : "null";
                sb.AppendLine($"  [{kvp.Key}]: {valueStr}");
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
