using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public class GlobalStates
    {
        private HashSet<IGlobalStateHandlerBase> States = new();

        public IEnumerable<IGlobalStateHandlerBase> GetStates()
        {
            return States;
        }
        public void Register(IGlobalStateHandlerBase state)
        {
            States.Add(state);
        }
    }
}