using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    public class UpdateTicker<T> : BlackListedTicker<T> where T : ITickable
    {

        public override void Tick()
        {
            foreach (var _tickable in Subscribed) _tickable.Tick();
        }


        public override float UpdatesPerSecond => 1 / Time.deltaTime;
        public override float DeltaTime => Time.deltaTime;
        public override HashSet<T> Subscribed { get; } = new();

        public UpdateTicker(HashSet<IGlobalGameState> blackList, int pr = 0) : base(blackList, pr)
        {
        }
    }
}