namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityStopBeingPlayer 
    {
        public IEntity Ent;
        public PlayerData Data;

        public OnEntityStopBeingPlayer(IEntity ent, PlayerData data)
        {
            Ent = ent;
            Data = data;
        }
    }
}
