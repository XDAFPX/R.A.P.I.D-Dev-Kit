namespace DAFP.TOOLS.ECS
{
    public interface IEntityComponent : ITickable
    {
        public void Register(Entity entity);
        public void OnInitializeInternal();
        public void OnStartInternal();
    }
}