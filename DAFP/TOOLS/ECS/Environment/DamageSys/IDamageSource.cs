using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.EventBus;
using Optional;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public interface IDamageSource
    {
        public Option<IEntity> Author { get; }
        public Option<IVectorBase> ImpactPos { get; }
    }

    public struct DamageSource : IDamageSource
    {
        public static DamageSource DEFAULT = new DamageSource(Option.None<IEntity>(), Option.None<IVectorBase>() );
        public DamageSource(Option<IEntity> author, Option<IVectorBase> impactPos)
        {
            Author = author;
            ImpactPos = impactPos;
        }

        public Option<IEntity> Author { get; }
        public Option<IVectorBase> ImpactPos { get; }
    }
}