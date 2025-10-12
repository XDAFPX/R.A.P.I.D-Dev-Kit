using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.ViewModel
{
    public interface IViewModel : IEntityPet, INameable, ISwitchable
    {
        public IViewModel Construct(IEntity owner);
    }
}