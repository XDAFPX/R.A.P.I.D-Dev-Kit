using JetBrains.Annotations;

namespace BDeshi.BTSM
{
    public interface ITransitionBase
    {
        /// <summary>
        /// Returns true if evaluation succeeds
        /// </summary>
        /// <returns></returns>
        bool Evaluate();
        IState SuccessState { get; }
        bool TakenLastTime { get; set; }
        bool TransitionToSameState { get; set; }
    }
    /// <summary>
    /// General interface for Transitions
    /// </summary>
    public interface ITransition<TState>: ITransitionBase
        where TState : IState
    {
        TState SuccessTypedState { get; }
        [CanBeNull] public System.Action OnTaken { get; }
    }

}