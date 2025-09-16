using System.Collections.Generic;
using BDeshi.BTSM;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public abstract class GlobalConfigStateHandler : GlobalStateHandler<IConfigState>,IGlobalConfigStateHandler
    {
        public GlobalConfigStateHandler(string defaultState,GlobalStates states) : base(defaultState,states)
        {
        }

        public abstract string GetDomain();
    }

    public class BasicConfigState : IConfigState
    {
        public BasicConfigState(string stateName)
        {
            StateName = stateName;
        }

        public string StateName { get; }
        public void EnterState()
        {
            
        }

        public void Tick()
        {
            
        }

        public void ExitState()
        {
            
        }

        public string Prefix { get; set; }
        public string FullStateName => StateName;
        public string Name => StateName;
        public HashSet<IState._stateTags> StateTags { get; }
        public IState Parent { get; set; }
    }
    public interface IConfigState : IState
    {
        
    }

    public interface IGlobalConfigStateHandler : IGlobalStateHandler<IConfigState>
    {
        public string GetDomain();
    }
}