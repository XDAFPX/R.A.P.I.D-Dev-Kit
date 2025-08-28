using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public class UpdateTicker<T> : BlackListedTicker<T> where T : ITickable
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


        public override float UpdatesPerSecond => 1 / Time.deltaTime;
        public override float DeltaTime => Time.deltaTime;
        public override HashSet<T> Subscribed { get; } = new();

        public UpdateTicker(HashSet<IGlobalGameState> blackList) : base(blackList)
        {
        }
    }
}