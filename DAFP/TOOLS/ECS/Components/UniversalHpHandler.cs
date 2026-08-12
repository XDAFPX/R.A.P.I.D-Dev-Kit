using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using MessagePipe;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine.Events;
using Zenject;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalHpHandler : EntityComponent, IHealthHandler
    {
        [Inject] private IPublisher<OnEntityHealthChangedEvent> e;
        public UnityEvent<OnEntityTakeDamageEvent> OnTakeDmg;
        public UnityEvent<OnEntityTakeHealingEvent> OnTakeHeal;
        

        protected override void OnInitialize()
        {
        }

        public void TakeDamage(IDamage damage)
        {
            if (Host is IDieable { Dead: true })
            {
                return;
            }

            GetHealth().TakeDamage(damage);
            broadcast_take_damage(damage);


            if (Host is IDieable { Dead: true } _postDamageCheck)
            {
                _postDamageCheck.Die(damage);
            }
            else
                Host.View.Do(new HurtAction(damage.Info)).Forget();
        }

        public void TakeHealing(IHealing healing)
        {
            if (Host is IDieable { Dead: true })
            {
                return;
            }

            GetHealth().TakeHealing(healing);

            broadcast_take_healing(healing);
            Host.View.Do(new HealAction(healing.Info)).Forget();
        }


        protected virtual IStat<uint> GetHealth()
        {
            return GameUtils.GetHpStats(Host).FirstOrDefault() ?? new QuikStat<uint>(1);
        }

        private void broadcast_take_damage(IDamage dmg)
        {
            broadcast_change(dmg);
            OnTakeDmg?.Invoke(new OnEntityTakeDamageEvent(Host, dmg));
        }

        private void broadcast_take_healing(IHealing healing)
        {
            broadcast_change(healing);
            OnTakeHeal?.Invoke(new OnEntityTakeHealingEvent(Host, healing));
        }

        private void broadcast_change(IHealthChange dmg)
        {
            e.Publish(new OnEntityHealthChangedEvent(Host, dmg));
        }


        public override ITicker EntityComponentTicker => World.EMPTY_TICKER;

        protected sealed override void OnTick()
        {
        }
    }
}