using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.ECS.DebugSystem;
using UGizmo;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalDebugSystem<TMessenger, TGizmos> : IDebugSys<TGizmos, TMessenger> where TGizmos :
        IGlobalGizmos
        where TMessenger :
        IMessenger
    {
        private List<IDebugDrawable> pets = new List<IDebugDrawable>();
        private List<IDebugSubSys> pets1 = new();

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
        public DebugDrawLayer GetSharedLayer { get; }


        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => pets;

        public void AddPet(IDebugSubSys pet)
        {
            if (pet == null) return;
            if (pets1.Contains(pet)) return;
            pets1.Add(pet);
        }

        public bool RemovePet(IDebugSubSys pet)
        {
            if (pet == null) return false;
            if (!pets1.Contains(pet)) return false;
            pets1.Remove(pet);
            return true;
        }

        IEnumerable<IDebugSubSys> IOwnerOf<IDebugSubSys>.Pets => pets1;

        public IEnumerable<object> AbsolutePets => pets.Union<object>(pets1);
    }
}