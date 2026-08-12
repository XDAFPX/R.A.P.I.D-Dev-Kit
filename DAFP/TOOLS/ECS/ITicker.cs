using System;
using System.Collections.Generic;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public interface ITicker : ITickerBase,IDisposable
    {
        public HashSet<ITickable> Subscribed { get; }
        void IDisposable.Dispose()
        {
            Subscribed.Clear();
        }
    }
}