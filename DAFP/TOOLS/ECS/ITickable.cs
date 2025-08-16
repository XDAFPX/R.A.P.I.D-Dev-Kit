namespace DAFP.TOOLS.ECS
{
    public interface ITickable
    {
        public void OnStart();
        public void Tick();
    }
}