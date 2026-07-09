using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.BigData;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    // Common base for health modifications (damage, healing, etc.)
    public interface IHealthChange<out TInfo> : IHealthChange where TInfo : struct, IHealthChangeInfo
    {
        TInfo Info { get; }
        IHealthChangeInfo IHealthChange.ChangeInfo => Info;
    }

    public interface IHealthChange
    {
        IHealthChangeInfo ChangeInfo { get; }
    }

    public interface IHealthChangeInfo
    {
        public Change Change { get; }
        public IStat<uint> Amount { get; }

        public IHaveGameplayTag Tag { get; }
    }
}