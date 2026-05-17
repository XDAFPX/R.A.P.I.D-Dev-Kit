using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public struct HealingInfo : IHealthChangeInfo
    {
        public static HealingInfo From(IStat<uint> val, IHealthChangeSource source, IHaveGameplayTag tag) =>
            new HealingInfo(val, source, tag);

        public static HealingInfo From(IStat<uint> val, IHaveGameplayTag tag) =>
            new HealingInfo(val, HealthChangeSource.DEFAULT, tag);

        public static HealingInfo From(IStat<uint> val) =>
            new HealingInfo(val, HealthChangeSource.DEFAULT, GameplayTagContainer.Empty);

        public static HealingInfo From(uint val) =>
            new HealingInfo(new QuikStat<uint>(val), HealthChangeSource.DEFAULT, GameplayTagContainer.Empty);

        public static HealingInfo DEFAULT =
            new HealingInfo(new QuikStat<uint>(0), HealthChangeSource.DEFAULT, GameplayTagContainer.Empty);

        public HealingInfo(IStat<uint> amount, IHealthChangeSource source, IHaveGameplayTag tag)
        {
            Amount = amount;
            Source = source;
            Tag = tag;
        }

        public IStat<uint> Amount { get; }
        public IHealthChangeSource Source { get; }
        public IHaveGameplayTag Tag { get; }
    }
}
