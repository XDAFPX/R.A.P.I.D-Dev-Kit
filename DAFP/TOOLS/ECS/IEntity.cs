using System.Collections.Generic;
using NUnit.Framework;

namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable
    {
        public HashSet<IEntityComponent> Components { get; }
        internal void Initialize();
        public bool HasInitialized { get; set; }
        public ITicker<IEntity> ITicker { get; set; }
    }
}