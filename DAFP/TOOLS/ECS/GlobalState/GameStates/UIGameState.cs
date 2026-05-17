using System.Linq;
using BDeshi.BTSM;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.GlobalState.CursorSates;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using Zenject;

namespace DAFP.TOOLS.ECS.GlobalState.GameStates
{
    public class UIGameState<TCursorState> : StateBase, IGameState where TCursorState : IGlobalCursorState, new()
    {
        public override string StateName { get; } = "UI";
        public override BtStatus LastStatus { get; } = BtStatus.Success;
        [Inject] private ICursorStateHandler cursor;
        [Inject] private ControllerManager controller_manager;

        public override void EnterState()
        {
            cursor.TransitionTo<TCursorState>();

            controller_manager.Controllers.OfType<UIInputController>().EnableAll();
        }

        public override void Tick()
        {
        }

        public override void ExitState()
        {
            controller_manager.Controllers.OfType<UIInputController>().DisableAll();
        }
    }
}