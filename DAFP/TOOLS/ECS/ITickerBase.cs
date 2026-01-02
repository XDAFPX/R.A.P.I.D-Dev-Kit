using System;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.GlobalState;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public interface ITickerBase : ITickable, IResetable, IComparable<ITickerBase>
    {
        public bool IsAllowedToTick(IGlobalGameState state);
        public float UpdatesPerSecond { get; }
        public float DeltaTime { get; }
        public void Remove(ITickable tickable);

        public int Priority { get; }

        int IComparable<ITickerBase>.CompareTo(ITickerBase other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}