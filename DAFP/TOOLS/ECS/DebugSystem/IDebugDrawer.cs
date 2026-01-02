using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using UGizmo;
using UGizmo.Internal;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugDrawer : IPet<IDebugDrawable>, IDrawable
    {
        public DebugDrawLayer Layer { get; set; }

        public void Initilize(IDebugSys<IGlobalGizmos, IMessenger> debugSys);
        void DrawInternal();


        void IDrawable.Draw()
        {
            if (Layer.Enabled == false)
                return;
            DrawInternal();
        }
    }
}