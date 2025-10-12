using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public class FixedUpdateTicker<T> : BlackListedTicker<T> where T : ITickable
    {
        public override void OnStart()
        {
            foreach (var _tickable in Subscribed)
            {
                _tickable.OnStart();
            }
        }

        public override void Tick()
        {
            foreach (var _tickable in Subscribed)
            {
                _tickable.Tick();
            }
        }


        public override float UpdatesPerSecond => 1 / Time.fixedDeltaTime;
        public override float DeltaTime => Time.fixedDeltaTime;
        public override HashSet<T> Subscribed { get; } = new();

        public FixedUpdateTicker(HashSet<IGlobalGameState> blackList,int pr =0) : base(blackList,pr)
        {
        }
    }
}