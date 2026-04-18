using DAFP.TOOLS.Common;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    public interface IHurtBoxController<T> : IOwnerOf<HurtGroup<T>>, IOwnedBy<IEntity>,IEntityComponent
    {
        
    }
}