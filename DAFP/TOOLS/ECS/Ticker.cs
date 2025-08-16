using System.Collections.Generic;

namespace DAFP.TOOLS.ECS
{
    public class Ticker<T> : ITicker<T> where T : ITickable
    {
        public Ticker(float updatesPerSecond)
        {
            UpdatesPerSecond = updatesPerSecond;
        }

        public virtual void OnStart()
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

        public float UpdatesPerSecond { get; }
        public float DeltaTime => 1 / UpdatesPerSecond;
        public HashSet<T> Subscribed { get; } = new HashSet<T>();
    }
}