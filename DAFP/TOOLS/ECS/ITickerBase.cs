using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.GlobalState;

namespace DAFP.TOOLS.ECS
{
    public interface ITickerBase : ITickable , IResetable
    {
        public bool IsAllowedToTick(IGlobalGameState state);
        public float UpdatesPerSecond { get; }
        public float DeltaTime { get; }
        public void Remove(ITickable tickable);
    }
}