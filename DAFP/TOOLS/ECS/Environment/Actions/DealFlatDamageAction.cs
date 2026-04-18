using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using Optional;

namespace DAFP.TOOLS.ECS.Environment.Actions
{
    [System.Serializable]
    public class DealFlatDamageAction : DealDamageAction
    {
        public uint Damage;

        public DealFlatDamageAction() : base(null, GameplayTagContainer.Empty)
        {
        }

        public DealFlatDamageAction(uint damage, GameplayTagContainer tags, Entity entity) : base(entity, tags)
        {
            Damage = damage;
            Tags = tags;
        }

        protected override void DealDamage(IDamageable damageable)
        {
            damageable.TakeDamage(new Damage(new DamageInfo(new QuikStat<uint>(Damage),
                new DamageSource(((IEntity)Owner).SomeNotNull(), Option.None<IVectorBase>()),
                Tags ?? GameplayTagContainer.Empty)));
        }
    }
}