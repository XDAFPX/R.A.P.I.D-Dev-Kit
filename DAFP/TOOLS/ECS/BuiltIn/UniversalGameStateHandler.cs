using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalGameStateHandler : GlobalGameStateHandler
    {
        [Inject]
        public UniversalGameStateHandler([Inject(Id = "DefaultGameState")] string defaultState,
            IGlobalCursorStateHandler cursorStateHandler, [Inject(Id = "GlobalStateBus")] IEventBus vus) : base(
            defaultState,
            cursorStateHandler, vus)
        {
        }

        private StateChangeRequest<IGlobalCursorState> lastCursorRequest;

        protected override IGlobalGameState[] GetPreBuildStates()
        {
            IGlobalGameState DefaultGamePlayState =
                new BasicGlobalGameState("PlayState",
                    () =>
                    {
                        cursorStateHandler.PopState(lastCursorRequest);
                        lastCursorRequest = cursorStateHandler.PushState(new CursorStateChangeRequest(
                            cursorStateHandler.GetState("Default"), 1, nameof(UniversalGameStateHandler),
                            CursorLockMode.Locked, false));
                    }, null, () => { });

            IGlobalGameState DefaultUIState =
                new BasicGlobalGameState("UIState",
                    () =>
                    {
                        cursorStateHandler.PopState(lastCursorRequest);
                        lastCursorRequest = cursorStateHandler.PushState(new CursorStateChangeRequest(
                            cursorStateHandler.GetState("Default"), 1, nameof(UniversalGameStateHandler),
                            CursorLockMode.None, true));
                    }, null, () => { });
            return new[]
            {
                DefaultGamePlayState, DefaultUIState
            };
        }

        public override Dictionary<string, object> Save()
        {
            return new Dictionary<string, object> { { "CurrentState", Current().StateName } };
        }

        public override void Load(Dictionary<string, object> save)
        {
            if (save.TryGetValue("CurrentState", out var _value))
            {
                ResetToDefault();
                PushState(new StateChangeRequest<IGlobalGameState>(GetState(_value as string), 0, "SaveSys"));
            }
        }
    }
}