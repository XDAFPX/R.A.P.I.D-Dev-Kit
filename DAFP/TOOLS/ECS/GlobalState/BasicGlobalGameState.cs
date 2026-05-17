using System;
using System.Collections.Generic;
using BDeshi.BTSM;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public class BasicGameState : ModularState, IGameState
    {
        public BasicGameState(string stateName, Action onEnter = null, Action onTick = null, Action onExit = null,
            HashSet<IState._stateTags> tags = null) : base(stateName, onEnter, onTick, onExit, tags)
        {
        }
    }
}