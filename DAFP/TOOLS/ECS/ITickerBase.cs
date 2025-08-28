using DAFP.TOOLS.ECS.GlobalState;

namespace DAFP.TOOLS.ECS
{
    public interface ITickerBase : ITickable
    {
        public bool IsAllowedToTick(IGlobalGameState state);
        public float UpdatesPerSecond { get; }
        public float DeltaTime { get; }
    }
}