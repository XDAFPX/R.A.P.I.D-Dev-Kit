namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityBecomePlayerEvent
    {
        public IEntity Ent;
        public PlayerData Data;

        public OnEntityBecomePlayerEvent(IEntity ent, PlayerData data)
        {
            Ent = ent;
            Data = data;
        }
    }
}