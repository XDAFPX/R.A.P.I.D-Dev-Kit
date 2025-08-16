using System.Collections.Generic;

namespace DAFP.TOOLS.ECS
{
    public interface ITicker<T> : ITickable, ITickerBase where T : ITickable
    {
        public HashSet<T> Subscribed { get; }
    }
}