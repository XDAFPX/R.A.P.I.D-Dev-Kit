using System.Collections.Generic;
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


        private readonly HashSet<IOwnedBy<IDebugDrawable>> _pets = new();

        private ISet<IOwnedBy<IDebugSys<IGlobalGizmos, IMessenger>>> pets =
            new HashSet<IOwnedBy<IDebugSys<IGlobalGizmos, IMessenger>>>();

        IEnumerable<IOwnedBy<IDebugDrawable>> IOwnerOf<IDebugDrawable>.Pets => _pets;
        void IOwnerOf<IDebugDrawable>.AddPet(IOwnedBy<IDebugDrawable> pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return;
            _pets.Add(pet);
        }
        bool IOwnerOf<IDebugDrawable>.RemovePet(IOwnedBy<IDebugDrawable> pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return false;
            return _pets.Remove(pet);
        }

        IEnumerable<IOwnedBy<IDebugSys<IGlobalGizmos, IMessenger>>> IOwnerOf<IDebugSys<IGlobalGizmos, IMessenger>>.Pets => pets;
        void IOwnerOf<IDebugSys<IGlobalGizmos, IMessenger>>.AddPet(IOwnedBy<IDebugSys<IGlobalGizmos, IMessenger>> pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return;
            pets.Add(pet);
        }
        bool IOwnerOf<IDebugSys<IGlobalGizmos, IMessenger>>.RemovePet(IOwnedBy<IDebugSys<IGlobalGizmos, IMessenger>> pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return false;
            return pets.Remove(pet);
        }

    }
}