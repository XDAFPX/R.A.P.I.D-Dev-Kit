using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using UGizmo;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugSubSys : IOwnedBy<IDebugSys<IGlobalGizmos,IMessenger>>
    {
        
    }
    public interface IDebugSys<out TGizmos, out TMessenger> : IDrawable, IOwnerOf<IDebugDrawable>,
        Zenject.ITickable,IOwnerOf<IDebugSubSys>
        where TGizmos : IGlobalGizmos where TMessenger : IMessenger
    {
        public TGizmos Gizmos { get; }
        public TMessenger Messenger { get; }
        public IList<DebugDrawLayer> Layers { get; }
        public DebugDrawLayer GetSharedLayer { get; }

        void Zenject.ITickable.Tick()
        {
            Messenger.Tick();
            Draw();
        }

        void IDrawable.Draw()
        {
            foreach (var _ownable in ((IOwnerOf<IDebugDrawable>)this).Pets)
                    _ownable.Draw();
        }

    }
}