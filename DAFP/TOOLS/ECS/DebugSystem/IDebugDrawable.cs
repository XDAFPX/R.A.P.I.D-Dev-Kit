using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using UGizmo;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugDrawable : IOwner<IDebugDrawable>, IDrawable
    {
        IDebugSys<IGlobalGizmos, IMessenger> DebugSystem { get; }

        void IDrawable.Draw()
        {
            foreach (var _ownable in Pets)
                if (_ownable is IDebugDrawer drawer)
                    drawer.Draw();
        }
    }
}