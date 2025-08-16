namespace DAFP.TOOLS.ECS
{
    public interface ITickerBase
    {
        public float UpdatesPerSecond { get; }
        public float DeltaTime { get; }
    }
}