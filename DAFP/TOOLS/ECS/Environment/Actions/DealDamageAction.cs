using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.TriggerSys;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Actions
{
    public abstract class DealDamageAction : ITriggerAction, IActionUpon<IEntity>, IActionUpon<GameObject>
    {
        public Entity Owner;

        public GameplayTagContainer Tags;

        protected DealDamageAction(Entity owner, GameplayTagContainer tags)
        {
            Owner = owner;
            Tags = tags;
        }

        public void Act(TriggerContext target)
        {
            Act(target.Target.GameObject);
        }

        public void Act(IEntity target)
        {
            if (target is IDamageable _damageable)
            {
                DealDamage(_damageable);
            }
        }


        public void Act(GameObject target)
        {
            if(target==null)
                return;
            if (target.TryGetComponent<IDamageable>(out var _damageable))
            {
                DealDamage(_damageable);
            }
        }

        protected abstract void DealDamage(IDamageable damageable);
        public void Act()
        {
            return;
        }
    }
}