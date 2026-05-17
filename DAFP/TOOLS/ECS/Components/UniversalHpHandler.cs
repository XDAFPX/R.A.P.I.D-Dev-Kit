using System.Linq;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine.Events;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalHpHandler : EntityComponent, IHealthHandler
    {
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

            BroadcastTakeDamage(damage);
            GetHealth().TakeDamage(damage);

            if (Host is IDieable { Dead: true } _postDamageCheck)
            {
                _postDamageCheck.Die(damage);
            }
            else
                Host.View.Do(new IAnimAction.HurtAction(damage.Info));
        }

        public void TakeHealing(IHealing healing)
        {
            if (Host is IDieable { Dead: true })
            {
                return;
            }

            BroadcastTakeHealing(healing);
            GetHealth().TakeHealing(healing);

            Host.View.Do(new IAnimAction.HealAction(healing.Info));
        }


        protected virtual IStat<uint> GetHealth()
        {
            return GameUtils.GetHpStats(Host).FirstOrDefault() ?? new QuikStat<uint>(1);
        }

        protected virtual void BroadcastTakeDamage(IDamage dmg)
        {
            var e = new OnEntityTakeDamageEvent(Host, dmg);
            OnTakeDmg?.Invoke(e);
            Host.BroadcastEvent(e);
        }

        protected virtual void BroadcastTakeHealing(IHealing heal)
        {
            var _e = new OnEntityTakeHealingEvent(Host, heal);
            OnTakeHeal?.Invoke(_e);
            Host.BroadcastEvent(_e);
        }

        public override ITickerBase EntityComponentTicker => World.EMPTY_TICKER;

        protected sealed override void OnTick()
        {
        }
    }
}