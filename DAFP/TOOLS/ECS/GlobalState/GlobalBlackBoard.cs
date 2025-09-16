using System.Collections.Generic;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public class GlobalBlackBoard : IGlobalBoard
    {
        protected Dictionary<string, object> Data;

        public GlobalBlackBoard(Dictionary<string, object> data)
        {
            Data = data;
        }

        public void Set<T>(string key, T value)
        {
            Data[key] = value;
        }

        public T Get<T>(string key)
        {
            return (T)Data.GetValueOrDefault(key);
        }

        public bool Has(string key)
        {
            return Data.ContainsKey(key);
        }

        public Dictionary<string, object> Save()
        {
            return Data;
        }

        public void Load(Dictionary<string, object> save)
        {
            Data = save;
        }
    }
}