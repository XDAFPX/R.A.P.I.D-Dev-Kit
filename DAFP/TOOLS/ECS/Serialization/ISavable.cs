using System.Collections.Generic;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISavable
    {
        public Dictionary<string, object> Save();
        public void Load(Dictionary<string, object> save);
    }
}