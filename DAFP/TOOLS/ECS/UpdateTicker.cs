using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public class UpdateTicker : BlackListedTicker
    {

        public override void Tick()
        {
            foreach (var _tickable in Subscribed) _tickable.Tick();
        }


        public override float UpdatesPerSecond => 1 / Time.deltaTime;
        public override float DeltaTime => Time.deltaTime;
        public override HashSet<ITickable> Subscribed { get; } = new();

        public UpdateTicker(HashSet<IGameState> blackList, int pr = 0) : base(blackList, pr)
        {
        }
    }
}