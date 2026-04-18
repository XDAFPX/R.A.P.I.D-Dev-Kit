using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using Optional;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Actions
{
    [System.Serializable]
    public class DealStatDamageAction : DealDamageAction
    {
        public string PathToStat;

        public DealStatDamageAction() : base(null,GameplayTagContainer.Empty)
        {
        }

        public DealStatDamageAction(string pathToStat, GameplayTagContainer container,Entity ent) : base(ent,container)
        {
            PathToStat = pathToStat;
        }

        protected override void DealDamage(IDamageable damageable)
        {
            if (Owner == null)
            {
                Debug.LogWarning("No Owner found of DealDamageAction!");
                return;
            }

            if (!Owner.Stats.Has(PathToStat))
            {
                Debug.LogWarning($"No stat of ({PathToStat}) found of DealDamageActionStat!");
                return;
            }
            damageable.TakeDamage(new Damage(new DamageInfo(Owner.Stats.Get(PathToStat,(() => new QuikStat<uint>(123))),
                new DamageSource(((IEntity)Owner).SomeNotNull(), Option.None<IVectorBase>()),
                Tags ?? GameplayTagContainer.Empty)));
        }
    }
}