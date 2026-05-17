namespace DAFP.TOOLS.ECS.GlobalState.Events
{
    public struct OnGameStateChanged
    {
        public IGameState Previous { get; init; }
        public IGameState New { get; init; }
    }
}