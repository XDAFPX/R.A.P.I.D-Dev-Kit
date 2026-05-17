using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.EventBus;
using Optional;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public interface IHealthChangeSource
    {
        public Option<IEntity> Author { get; }
        public Option<IVector> ImpactPos { get; }
    }

    public struct HealthChangeSource : IHealthChangeSource
    {
        public static HealthChangeSource DEFAULT = new HealthChangeSource(Option.None<IEntity>(), Option.None<IVector>() );
        public HealthChangeSource(Option<IEntity> author, Option<IVector> impactPos)
        {
            Author = author;
            ImpactPos = impactPos;
        }

        public Option<IEntity> Author { get; }
        public Option<IVector> ImpactPos { get; }
    }
}