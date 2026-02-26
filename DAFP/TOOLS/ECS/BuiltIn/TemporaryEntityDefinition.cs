using Bdeshi.Helpers.Utility;
using UnityEngine;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class TemporaryEntityDefinition : EntityComponent,IListener<ICommonEntityEvent.EntitySpawnFromAssetsEvent>
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

        protected override void OnStart()
        {
        }

        public void React(in ICommonEntityEvent.EntitySpawnFromAssetsEvent e)
        {
            Timer.reset();
        }
    }
}