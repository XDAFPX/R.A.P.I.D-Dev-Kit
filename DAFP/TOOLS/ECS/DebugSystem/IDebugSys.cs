using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using UGizmo;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugSys<out TGizmos, out TMessenger> : IDrawable, IOwner<IDebugDrawable>,
        Zenject.ITickable, IOwner<IDebugSys<IGlobalGizmos, IMessenger>>
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
            foreach (var _ownable in ((IOwner<IDebugDrawable>)this).Pets)
            {
                if (_ownable is IDrawable drawable)
                {
                    drawable.Draw();
                }
            }
        }

        void IOwner<IDebugDrawable>.AddPet(IOwnable<IDebugDrawable> pet)
        {
            if (pet == this)
                return;
            if (pet == null)
                return;
            ((IOwner<IDebugDrawable>)this).Pets.Add(pet);
        }

        bool IOwner<IDebugDrawable>.RemovePet(IOwnable<IDebugDrawable> pet)
        {
            if (pet == this)
                return false;
            if (pet == null)
                return false;
            ((IOwner<IDebugDrawable>)this).Pets.Remove(pet);

            return true;
        }
    }
}