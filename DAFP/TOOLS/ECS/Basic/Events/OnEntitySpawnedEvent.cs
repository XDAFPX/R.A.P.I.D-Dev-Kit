namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntitySpawnedEvent
    {
        public OnEntitySpawnedEvent(IEntity host)
        {
            Host = host;
        }

        public IEntity Host { get; }
    }
}
