using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS
{
    public interface IEntityComponent : ITickable,IGameObjectProvider
    {
        public void Register(IEntity entity);
        public void Initialize();
        public void OnStartInternal();
    }
}