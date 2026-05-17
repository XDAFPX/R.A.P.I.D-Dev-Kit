using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalGameStateHandler : GlobalGameStateHandler
    {
        [Inject(Id = "DefaultGameState")] public override IGameState Default { get; }
    }
}