using System;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using MessagePipe;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalDeathHandler : EntityComponent, IDeathHandler
    {
        public override ITicker EntityComponentTicker => World.EmptyTicker;
        [Inject] protected IPublisher<OnEntityDieEvent> DieEvent;

        [field: SerializeField] protected virtual DeathHandleStrategy Strategy { get; set; }

        public UnityEvent<OnEntityDieEvent> Event;

        protected override void OnInitialize()
        {
        }

        public void Die(IDamage lethal)
        {
            trigger_event(lethal);
            HandleDeath(lethal);
        }

        protected virtual void HandleDeath(IDamage lethal)
        {
            switch (Strategy)
            {
                case DeathHandleStrategy.PlayDeathAnimDestroySelf:
                    Host.View.Do(new DieAction(lethal.Info)).ContinueWith(() => Adam.Destroy(Host)).Forget();
                    break;
                case DeathHandleStrategy.DestroySelf:
                    Adam.Destroy(Host).Forget();
                    break;
                case DeathHandleStrategy.PlayDeathAnim:
                    Host.View.Do(new DieAction(lethal.Info)).Forget();
                    break;
                case DeathHandleStrategy.DisableView:
                    Host.View.DisableAll();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void trigger_event(IDamage lethal)
        {
            var _e = new OnEntityDieEvent(Host, lethal);
            DieEvent.Publish(_e);
            Event.Invoke(_e);
        }

        protected sealed override void OnTick()
        {
        }

        protected enum DeathHandleStrategy
        {
            PlayDeathAnimDestroySelf,
            DestroySelf,
            PlayDeathAnim,
            DisableView
        }
    }
}