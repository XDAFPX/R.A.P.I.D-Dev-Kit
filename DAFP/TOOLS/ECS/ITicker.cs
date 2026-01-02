using System;
using System.Collections.Generic;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public interface ITicker<T> : ITickerBase where T : ITickable
    {
        public HashSet<T> Subscribed { get; }
    }
}