namespace DAFP.TOOLS.ECS.GlobalState.Events
{
    public struct OnGameStateChangedEvent
    {
        public OnGameStateChangedEvent(IGameStateHandler handler, IGameState @new, IGameState previous)
        {
            Handler = handler;
            New = @new;
            Previous = previous;
        }

        public IGameStateHandler Handler { get; }
        public IGameState Previous { get;  }
        public IGameState New { get;  }
    }
}