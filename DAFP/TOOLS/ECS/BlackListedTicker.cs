using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.ECS.GlobalState;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public abstract class BlackListedTicker<T> : ITicker<T> where T : ITickable
    {
        protected HashSet<IGlobalGameState> BlackList;

        protected BlackListedTicker(HashSet<IGlobalGameState> blackList, int priority)
        {
            BlackList = blackList;
        }

        public bool IsAllowedToTick(IGlobalGameState state)
        {
            if (BlackList.Contains(state))
                return false;
            return true;
        }

        public abstract void Tick();
        public abstract float UpdatesPerSecond { get; }
        public abstract float DeltaTime { get; }

        public void Remove(ITickable tickable)
        {
            var tt = Subscribed.FirstOrDefault(tickable1 => tickable1.Equals(tickable));
            if (tt != null) Subscribed.Remove(tt);
        }

        public abstract HashSet<T> Subscribed { get; }
        public int Priority { get; }

        public void ResetToDefault()
        {
            Subscribed.Clear();
        }
    }
}