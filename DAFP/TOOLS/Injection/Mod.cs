using DAFP.TOOLS.Common.TextSys;
using UnityEngine;

namespace DAFP.TOOLS.Injection
{
    public abstract class Mod : ScriptableObject, IMod
    {
        public string Name
        {
            get => name;
            set => name = value;
        }


        public abstract IMessage Description { get; set; }
        public abstract string Author { get; set; }

        public virtual void Dispose() {}
    }
}