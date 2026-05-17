using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    // Common base for health modifications (damage, healing, etc.)
    public interface IHealthChange<out TInfo> where TInfo : struct, IHealthChangeInfo
    {
        TInfo Info { get; }
    }

    public interface IHealthChangeInfo
    {
        public IStat<uint> Amount { get; }

        public IHaveGameplayTag Tag { get; }
    }
}