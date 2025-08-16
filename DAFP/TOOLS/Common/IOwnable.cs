using DAFP.TOOLS.ECS;

namespace DAFP.TOOLS.Common
{
    public interface IOwnable
    {
        IEntity GetCurrentOwner();
        void ChangeOwner(IEntity newOwner);
    }
}