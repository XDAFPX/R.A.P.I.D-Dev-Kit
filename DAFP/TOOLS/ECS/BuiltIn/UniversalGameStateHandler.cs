using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalGameStateHandler : GlobalGameStateHandler
    {
        [Inject]
        public UniversalGameStateHandler([Inject(Id = "DefaultGameState")] string defaultState,
            IGlobalCursorStateHandler cursorStateHandler) : base(defaultState, cursorStateHandler)
        {
        }

        protected override IGlobalGameState[] GetPreBuildStates()
        {
            IGlobalGameState DefaultGamePlayState =
                new BasicGlobalGameState("PlayState",
                    (() =>
                    {
                        Debug.Log("STAAAAAAAAA");
                        cursorStateHandler.PushState(new CursorStateChangeRequest(
                            cursorStateHandler.GetState("Default"), 1, nameof(UniversalGameStateHandler),
                            CursorLockMode.Locked, false));
                    }), null, (() => { }));

            IGlobalGameState DefaultUIState =
                new BasicGlobalGameState("UIState",
                    (() =>
                    {
                        cursorStateHandler.PushState(new CursorStateChangeRequest(
                            cursorStateHandler.GetState("Default"), 1, nameof(UniversalGameStateHandler),
                            CursorLockMode.None, true));
                    }), null, (() => { }));
            return new[]
            {
                DefaultGamePlayState, DefaultUIState
            };
        }
    }
}