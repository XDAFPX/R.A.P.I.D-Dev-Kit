using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public class Ticker<T> : BlackListedTicker<T> where T : ITickable
    {
        public Ticker(float updatesPerSecond, HashSet<IGlobalGameState> blackList, int pr = 0) : base(blackList, pr)
        {
            UpdatesPerSecond = updatesPerSecond;
        }



        public override void Tick()
        {
            foreach (var _tickable in Subscribed) _tickable.Tick();
        }

        public float Elapsed = 0;
        public override float UpdatesPerSecond { get; }
        public override float DeltaTime => 1 / UpdatesPerSecond;
        public override HashSet<T> Subscribed { get; } = new();
    }
}