using DAFP.TOOLS.ECS.GlobalState;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCursorStateHandler : GlobalCursorStateHandler
    {
        public UniversalCursorStateHandler(string defaultState) : base(defaultState)
        {
        }

        protected override IGlobalCursorState[] GetPreBuildStates()
        {
            return new[] { new BasicCursorState(this, null, "DefaultState") };
        }
    }
}