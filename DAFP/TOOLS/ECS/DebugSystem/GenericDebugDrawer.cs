using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using UGizmo;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class GenericDebugDrawer<T> : IDebugDrawer where T : class, IDebugDrawable
    {
        public List<IDebugDrawable> Owners { get; } = new();

        public DebugDrawLayer Layer { get; set; }

        protected IDebugSys<IGlobalGizmos, IMessenger> Sys;
        protected T Host => ((IOwnable<IDebugDrawable>)this).GetCurrentOwner() as T;

        public void Initilize(IDebugSys<IGlobalGizmos, IMessenger> debugSys)
        {
            Sys = debugSys;
            Layer = Sys.Layers.FindByName(GetLayerName()) as DebugDrawLayer ?? Sys.GetSharedLayer;
        }

        protected abstract string GetLayerName();
        public abstract void DrawInternal();
    }
}