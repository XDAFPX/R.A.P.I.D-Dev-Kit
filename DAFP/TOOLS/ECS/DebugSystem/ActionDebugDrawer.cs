using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using UGizmo;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public class ActionDebugDrawer : IDebugDrawer
    {
        private readonly string layerName;
        private readonly Action<IGlobalGizmos> draw;

        public ActionDebugDrawer(string layerName, Action<IGlobalGizmos> draw)
        {
            this.layerName = layerName;
            this.draw = draw;
        }

        public List<IDebugDrawable> Owners { get; } = new();
        public DebugDrawLayer Layer { get; set; }
        protected IDebugSys<IGlobalGizmos, IMessenger> Sys;

        public void Initilize(IDebugSys<IGlobalGizmos, IMessenger> debugSys)
        {
            Sys = debugSys;
            Layer = Sys.Layers.FindByName(layerName) as DebugDrawLayer ?? Sys.GetSharedLayer;
        }

        public void DrawInternal()
        {
            draw?.Invoke(Sys.Gizmos);
        }
    }
}