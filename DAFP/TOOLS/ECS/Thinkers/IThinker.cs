using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.DebugSystem;
using RapidLib.DAFP.TOOLS.Common;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public interface IThinker :  IPetOf<IDebugDrawable,IDebugDrawable>, IDebugDrawable,
        IPetOwnerTreeOf<IThinker>, ISubscriber
    {
        bool DIInjected { get; }
        bool HasInitialized { get; }
        void Initialize(IEntity host);
        void Tick(IEntity host, ITickerBase ticker);
        void Dispose(IEntity host);
    }
}