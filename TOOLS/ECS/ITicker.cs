using System.Collections.Generic;

namespace DAFP.TOOLS.ECS
{
    public interface ITicker<T> : ITickable where T : ITickable
    {
        public float UpdatesPerSecond { get; }
        public HashSet<T> Subscribed { get; }
    }
}