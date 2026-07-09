using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalGameStateHandler : GameStateHandler
    {
        [Inject(Id = "DefaultGameState")] public override IGameState Default { get; }
    }
}