using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;

namespace DAFP.TOOLS.ECS.ViewModel
{
    public interface IViewModel : IPetOf<IEntity,IViewModel>, INameable, ISwitchable
    {
        public IViewModel InitOwner(IEntity owner);
        public HurtGroup<IEntity> GetHurtGroup(IEntity owner);
        public interface IViewDoHurt :IViewModel
        {
            public IViewModel DoHurt();
        }
    }
}