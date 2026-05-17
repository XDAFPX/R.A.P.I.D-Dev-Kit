using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.ECS.Basic.Events;
using UnityEngine;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class TemporaryEntityDefinition : EntityComponent, IListener<OnEntitySpawnedEvent>
    {
        public FiniteTimer Timer;
        protected override void OnTick()
        {
            if (Timer.tryCompleteTimer(EntityComponentTicker.DeltaTime))
            {
                Despawn();
            }
        }

        public void Despawn()
        {
            Host.Remove(new EntityRemovalReason.VisualEntityRemovalReason());
        }

        protected override void OnInitialize()
        {
            Host.Bus.Subscribe(this);
        }


        public void React(in OnEntitySpawnedEvent e)
        {
            Timer.reset();
        }
    }
}