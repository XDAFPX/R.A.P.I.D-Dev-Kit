using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;

namespace DAFP.TOOLS.ECS
{
    public abstract class BlackListedTicker<T> : ITicker<T> where T : ITickable
    {
        public abstract void OnStart();
        protected HashSet<IGlobalGameState> BlackList;

        protected BlackListedTicker(HashSet<IGlobalGameState> blackList)
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
        public abstract HashSet<T> Subscribed { get; }
    }
}