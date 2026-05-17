using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.ViewModel
{
    public interface IViewModel : IPetOf<IEntity, IViewModel>, INameable, ISwitchable
    {
        public IViewModel InitOwner(IEntity owner);
        public HurtGroup<IEntity> GetHurtGroup(IEntity owner);

        public Compatability Parse(IAnimAction action);
        public Compatability Do(IAnimAction action) => Parse(action);
    }
}