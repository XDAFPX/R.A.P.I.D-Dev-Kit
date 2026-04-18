using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using UGizmo;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalDebugSystem<TMessenger, TGizmos> : IDebugSys<TGizmos, TMessenger> where TGizmos :
        IGlobalGizmos
        where TMessenger :
        IMessenger
    {
        private List<IDebugDrawable> debugDrawers = new List<IDebugDrawable>();
        private List<IDebugSubSys> subSystems = new();

        [Inject]
        public UniversalDebugSystem(TMessenger messenger, TGizmos gizmos, IList<DebugDrawLayer> layers)
        {
            Messenger = messenger;
            if (Messenger is IOwnedBy<IDebugSys<IGlobalGizmos, IMessenger>> ownable)
                ownable.ChangeOwner((IDebugSys<IGlobalGizmos, IMessenger>)this);
            Gizmos = gizmos;
            Layers = layers;
        }

        public TGizmos Gizmos { get; }
        public TMessenger Messenger { get; }
        public IList<DebugDrawLayer> Layers { get; }
        public DebugDrawLayer GetSharedLayer => new DebugDrawLayer("SHARED", Application.isEditor);


        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => debugDrawers;

        public void AddPet(IDebugDrawable pet)
        {
            GameUtils.AddPet(pet, ref debugDrawers);
        }

        public bool RemovePet(IDebugDrawable pet)
        {
            return GameUtils.RemovePet(pet, ref debugDrawers);
        }

        public void AddPet(IDebugSubSys pet)
        {
            if (pet == null) return;
            if (subSystems.Contains(pet)) return;
            subSystems.Add(pet);
        }

        public bool RemovePet(IDebugSubSys pet)
        {
            if (pet == null) return false;
            if (!subSystems.Contains(pet)) return false;
            subSystems.Remove(pet);
            return true;
        }

        IEnumerable<IDebugSubSys> IOwnerOf<IDebugSubSys>.Pets => subSystems;

        public IEnumerable<object> AbsolutePets => debugDrawers.Union<object>(subSystems);
    }
}