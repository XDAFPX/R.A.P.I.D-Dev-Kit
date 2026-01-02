using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.ViewModel
{
    public interface IViewModel : IEntityPet, INameable, ISwitchable
    {
        public IViewModel InitOwner(IEntity owner);
        public interface IViewDoHurt :IViewModel
        {
            public IViewModel DoHurt();
        }
    }
}