using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using UGizmo;
using UGizmo.Internal;
using UnityEditor.Graphs;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class EntityComponentDebugDrawer<TEntComp>
        : IDebugDrawer where TEntComp : IEntityComponent
    {
        public List<IDebugDrawable> Children { get; } = new();
        public List<IDebugDrawable> Owners { get; } = new();

        public DebugDrawLayer Layer { get; set; }

        protected IDebugSys<IGlobalGizmos, IMessenger> Sys;
        protected IEntity Host => ((IOwnedBy<IDebugDrawable>)this).GetCurrentOwner() as IEntity;
        protected TEntComp Component;

        protected EntityComponentDebugDrawer(TEntComp component)
        {
            Component = component;
        }

        protected abstract string GetLayerName();

        public void InitilizeDebugDrawer(IDebugSys<IGlobalGizmos, IMessenger> debugSys)
        {
            Sys = debugSys;
            Layer = Sys.Layers.FindByName(GetLayerName()) as DebugDrawLayer ?? Sys.GetSharedLayer;
        }

        public abstract void DrawInternal();
        public IDebugSys<IGlobalGizmos, IMessenger> DebugSystem => Sys;
    }
}