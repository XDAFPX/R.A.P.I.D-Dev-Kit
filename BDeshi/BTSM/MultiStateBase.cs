using System.Collections.Generic;
using System.Linq;

namespace BDeshi.BTSM
{
    public abstract class MultiStateBase : IMultiState
    {
        public abstract void EnterState();
        public abstract void Tick();
        public abstract void ExitState();
        public string Prefix { get; set; }
        public string FullStateName => $"<{stateName}>";
        public HashSet<IState._stateTags> StateTags => StateTagsInternal;
        protected HashSet<IState._stateTags> StateTagsInternal;

        private readonly string stateName;
        public string Name => this.GetType().Name;
        public IState Parent { get; set; }
        protected List<IState> Children;

        protected MultiStateBase(List<IState> children, HashSet<IState._stateTags> stateTagsInternal, string stateName)
        {
            Children = children;
            StateTagsInternal = stateTagsInternal;
            this.stateName = stateName;
        }

        public IEnumerable<IState> GetChildStates()
        {
            return Children;
        }

        public abstract IState GetActiveState { get; }
    }
}