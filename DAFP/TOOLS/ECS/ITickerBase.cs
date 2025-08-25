namespace DAFP.TOOLS.ECS
{
    public interface ITickerBase : ITickable
    {
        public float UpdatesPerSecond { get; }
        public float DeltaTime { get; }
    }
}