using System.Collections.Generic;
using NRandom;
using UnityEngine;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public class UtillGlobalBoard : GlobalBlackBoard,Zenject.ITickable
    {
        public UtillGlobalBoard() : base(new Dictionary<string, object>()
        {
            { "FUN1", Mathf.RoundToInt(Random.value * 777) },
            { "FUN2", RandomEx.Shared.NextUInt() },
            { "FUN3", RandomEx.Shared.NextInt() },
            { "GameTime", 0f },
        })
        {
        }

        public void Tick()
        {
            Data["GameTime"] = (float)Data["GameTime"] + Time.deltaTime;
        }
    }
}