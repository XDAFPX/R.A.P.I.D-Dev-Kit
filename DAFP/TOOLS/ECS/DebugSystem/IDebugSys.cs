using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using UGizmo;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugSys<out TGizmos, out TMessenger> : IDrawable, IOwnerOf<IDebugDrawable>,
        Zenject.ITickable, IOwnerOf<IDebugSys<IGlobalGizmos, IMessenger>>
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
                if (_ownable is IDrawable drawable)
                    drawable.Draw();
        }

        void IOwnerOf<IDebugDrawable>.AddPet(IOwnedBy<IDebugDrawable> pet)
        {
            if (pet == this)
                return;
            if (pet == null)
                return;
            ((IOwnerOf<IDebugDrawable>)this).AddPet(pet);
        }

        bool IOwnerOf<IDebugDrawable>.RemovePet(IOwnedBy<IDebugDrawable> pet)
        {
            // default interface helper cannot mutate enumerable; let concrete implementations handle removal
            return false;
        }
    }
}