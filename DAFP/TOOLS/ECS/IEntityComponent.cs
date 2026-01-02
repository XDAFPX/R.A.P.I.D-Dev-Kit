using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.DebugSystem;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public interface IEntityComponent : ITickable, IGameObjectProvider
    {
        public ITickerBase EntityComponentTicker { get; }
        public void Register(IEntity entity);
        public void Initialize();
        public void OnStartInternal();
        public IEnumerable<IDebugDrawer> SetupDebugDrawers();
    }
}