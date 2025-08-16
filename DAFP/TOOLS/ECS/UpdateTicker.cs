using System.Collections.Generic;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public class UpdateTicker<T> : ITicker<T> where T : ITickable
    {
        public void OnStart()
        {
            foreach (var _tickable in Subscribed)
            {
                _tickable.OnStart();
            }
        }

        public void Tick()
        {
            foreach (var _tickable in Subscribed)
            {
                _tickable.Tick();
            }
        }

        public float UpdatesPerSecond => 1 / Time.deltaTime;
        public float DeltaTime => Time.deltaTime;
        public HashSet<T> Subscribed { get; } = new();
    }
}