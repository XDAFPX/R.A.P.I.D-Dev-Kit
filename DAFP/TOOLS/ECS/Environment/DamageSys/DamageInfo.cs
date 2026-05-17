using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public struct DamageInfo : IHealthChangeInfo
    {
        public static DamageInfo From(IStat<uint> val, IHaveGameplayTag tag) =>
            new DamageInfo(val, HealthChangeSource.DEFAULT,  tag);

        public static DamageInfo From(IStat<uint> val) =>
            new DamageInfo(val, HealthChangeSource.DEFAULT,GameplayTagContainer.Empty); 

        public static DamageInfo From(uint val) =>
            new DamageInfo(new QuikStat<uint>(val), HealthChangeSource.DEFAULT,GameplayTagContainer.Empty); 

        public static DamageInfo DEFAULT =
            new DamageInfo(new QuikStat<uint>(0), HealthChangeSource.DEFAULT,GameplayTagContainer.Empty); 

        public DamageInfo(IStat<uint> damage, IHealthChangeSource source, IHaveGameplayTag tag)
        {
            Amount = damage;
            Source = source;
            Tag = tag;
        }

        public IStat<uint> Amount { get; }
        public IHealthChangeSource Source { get; }
        public IHaveGameplayTag Tag { get; }
    }
}