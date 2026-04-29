using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using UGizmo;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class GenericDebugDrawer<T> : IDebugDrawer where T : class, IDebugDrawable
    {
        public List<IDebugDrawable> Children { get; } = new();
        public List<IDebugDrawable> Owners { get; } = new();

        public DebugDrawLayer Layer { get; set; }

        protected IDebugSys<IGlobalGizmos, IConsoleMessenger> Sys;
        protected T Host => ((IOwnedBy<IDebugDrawable>)this).GetCurrentOwner() as T;

        public void InitilizeDebugDrawer(IDebugSys<IGlobalGizmos, IConsoleMessenger> debugSys)
        {
            Sys = debugSys;
            Layer = Sys.Layers.FindByName(GetLayerName()) as DebugDrawLayer ?? Sys.GetSharedLayer;
        }

        protected abstract string GetLayerName();
        public abstract void DrawInternal();
        public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem => Sys;
    }
}