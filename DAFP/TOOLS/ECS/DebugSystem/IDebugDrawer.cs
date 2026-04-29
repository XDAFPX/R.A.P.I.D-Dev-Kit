using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using RapidLib.DAFP.TOOLS.Common;
using UGizmo;
using UGizmo.Internal;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugDrawer : IOwnedBy<IDebugDrawable>, IDrawable,IDebugDrawable, IPetOwnerTreeOf<IDebugDrawable>
    {
        public DebugDrawLayer Layer { get; set; }

        public void InitilizeDebugDrawer(IDebugSys<IGlobalGizmos, IConsoleMessenger> debugSys);
        void DrawInternal();


        void IDrawable.Draw()
        {
            if (Layer.Enabled == false)
                return;
            DrawInternal();
        }
    }
}