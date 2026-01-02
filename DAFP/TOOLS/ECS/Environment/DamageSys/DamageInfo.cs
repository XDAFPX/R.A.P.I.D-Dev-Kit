using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public struct DamageInfo
    {
        public static DamageInfo From(IStat<uint> val, IHaveGameplayTag tag) =>
            new DamageInfo(val, DamageSource.DEFAULT,  tag);

        public static DamageInfo From(IStat<uint> val) =>
            new DamageInfo(val, DamageSource.DEFAULT,GameplayTagContainer.Empty); 

        public static DamageInfo From(uint val) =>
            new DamageInfo(new QuikStat<uint>(val), DamageSource.DEFAULT,GameplayTagContainer.Empty); 

        public static DamageInfo DEFAULT =
            new DamageInfo(new QuikStat<uint>(0), DamageSource.DEFAULT,GameplayTagContainer.Empty); 

        public DamageInfo(IStat<uint> damage, IDamageSource source, IHaveGameplayTag tag)
        {
            Damage = damage;
            Source = source;
            Tag = tag;
        }

        public IStat<uint> Damage { get; }
        public IDamageSource Source { get; }
        public IHaveGameplayTag Tag { get; }
    }
}