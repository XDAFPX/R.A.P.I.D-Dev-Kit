using Bdeshi.Helpers.Utility;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class TemporaryEntityDefinition : EntityComponent
    {
        public FiniteTimer Timer;

        [Inject] private Adam adam;

        protected override void OnTick()
        {
            if (Timer.tryCompleteTimer(EntityComponentTicker.DeltaTime))
            {
                Despawn();
                
            }
        }

        public void Despawn()
        {
            adam.Destroy(Host).Forget();
        }

        protected override void OnInitialize()
        {
            Timer.reset();
        }


    }
}