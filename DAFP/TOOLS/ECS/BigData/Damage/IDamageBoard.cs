using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using Optional;

namespace DAFP.TOOLS.ECS.BigData.Damage
{
    public interface IDamageBoard : IStat<uint>
    {

        public IDamage Construct(Option<IEntity> ent, Option<IVectorBase> vec, IStat<uint> stat);

        public IHaveGameplayTag Tag { get; }
    }
}