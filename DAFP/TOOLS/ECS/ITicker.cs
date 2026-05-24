using System;
using System.Collections.Generic;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public interface ITicker : ITickerBase 
    {
        public HashSet<ITickable> Subscribed { get; }
    }
}