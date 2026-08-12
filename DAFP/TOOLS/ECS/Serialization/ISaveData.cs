using System.Collections.Generic;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISaveData 
    {
        bool TryGet(string key, out object value);
        void Set(string key, object value);
        IEnumerable<string> Keys { get; }
    }
}