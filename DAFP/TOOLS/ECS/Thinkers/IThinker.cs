using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.DebugSystem;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public interface IThinker : IPetOf<IDebugDrawable, IDebugDrawable>, IDebugDrawable,
        IPetOwnerTreeOf<IThinker>, INameable
    {
    }

    internal interface IThinkerLogic : IThinker
    {
        void Start(IEntity host);
        void End(IEntity host);
        void Tick(IEntity host, ITickerBase ticker);
        bool HasWoken { get; set; }
    }
}