using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BDeshi.BTSM;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using ModestTree;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public abstract class BaseThinker : ScriptableObject, IThinker
    {
        // -- Dependencies
        [Inject] public IDebugSys<IGlobalGizmos, IMessenger> DebugSystem { get; }
        [Inject(Id = "GlobalStateBus")] protected IEventBus GlobalStateBus;

        [Inject] protected DiContainer Injector;
        [Inject] protected World World;

        [Inject] protected ISaveSystem SaveSystem;
        // -- Fields
#if UNITY_EDITOR
        [field: SerializeField] public bool EditMode { get; set; }
#endif
        [SerializeField] private List<SerializableInterface<IThinker>> ChildThinkers;

        private List<IDebugDrawable> debugDrawOwners = new();
        protected List<IThinker> ParentThinkers = new();

        /*[field: SerializeField]*/
        public bool DIInjected => DebugSystem == null;

        public bool HasInitialized { get; private set; }
        // -- Core Methods

        public void Initialize(IEntity host)
        {
            host.Bus.Subscribe(this);
            AnimationNameCacheInitializer.InitializeCaches(this);
            InternalInitialize(host);
            init_debug_drawers(SetupDebugDrawers(host));
            initialize_children(host);
            HasInitialized = true;
        }


        public void Tick(IEntity host, ITickerBase ticker)
        {
            tick_children(host, ticker);
            InternalTick(host, ticker);
        }

        public void Dispose(IEntity host)
        {
            host.Bus.UnSubscribe(this);
            InternalDispose(host);
            de_init_debug_drawers();
            dispose_children(host);
            HasInitialized = false;
        }

        protected abstract void InternalInitialize(IEntity host);
        protected abstract void InternalTick(IEntity host, ITickerBase ticker);
        protected abstract void InternalDispose(IEntity host);
        protected abstract IEnumerable<IDebugDrawer> SetupDebugDrawers(IEntity host);

        private void de_init_debug_drawers()
        {
            if (debugDrawPets == null || debugDrawPets.IsEmpty())
                return;
            debugDrawPets.Clear();
            DebugSystem.RemovePet(this);
        }

        private void init_debug_drawers(IEnumerable<IDebugDrawer> pets)
        {
            if (pets == null)
                return;
            var _debugDrawers = pets as IDebugDrawer[] ?? pets.ToArray();
            if (_debugDrawers.IsEmpty())
                return;
            debugDrawPets = debugDrawPets.Union<IDebugDrawable>(_debugDrawers).ToList();
            var petsSnapshot = debugDrawPets.ToArray();

            foreach (var _ownable in petsSnapshot)
            {
                _ownable.ChangeOwner(this);
                if (_ownable is IDebugDrawer _drawer)
                    _drawer.InitilizeDebugDrawer(DebugSystem);
            }

            DebugSystem.AddPet(this);

            DebugSystem.AddPet(this);
        }

        private void tick_children(IEntity host, ITickerBase ticker)
        {
            foreach (var _child in ChildThinkers)
            {
                _child.Value.Tick(host, ticker);
            }
        }

        private void dispose_children(IEntity host)
        {
            foreach (var _child in ChildThinkers)
            {
                _child.Value.Dispose(host);
            }
        }

        private void initialize_children(IEntity host)
        {
            foreach (var _child in ChildThinkers)
            {
                (_child.Value).ChangeOwner((IThinker)this);
                _child.Value.Initialize(host);
            }
        }


        List<IDebugDrawable> IPetOf<IDebugDrawable, IDebugDrawable>.Owners => debugDrawOwners;

        private List<IDebugDrawable> debugDrawPets = new();
        public List<IThinker> Children => ((IOwnerOf<IThinker>)this).Pets.ToList();

        IEnumerable<IThinker> IOwnerOf<IThinker>.Pets => ChildThinkers.ToValues();

        public void AddPet(IThinker pet)
        {
            if (pet == null) return;
            if (ChildThinkers.ToValues().Contains(pet)) return;
            ChildThinkers.Add(new SerializableInterface<IThinker>(pet));
        }

        public bool RemovePet(IThinker pet)
        {
            if (pet == null) return false;
            if (!ChildThinkers.ToValues().Contains(pet)) return false;
            ChildThinkers.Remove(new SerializableInterface<IThinker>(pet));
            return true;
        }

        public IEnumerable<IDebugDrawable> Pets => debugDrawPets;

        List<IThinker> IPetOwnerTreeOf<IThinker>.Owners => ParentThinkers;

        IEnumerable<object> IOwnerBase.AbsolutePets => debugDrawPets.Union(ChildThinkers.ToValues());
        public void AddPet(IDebugDrawable pet)
        {
            if (pet == null) return;
            if (Pets.Contains(pet)) return;
            debugDrawPets.Add(pet);
        }

        public bool RemovePet(IDebugDrawable pet)
        {
            if (pet == null) return false;
            if (!Pets.Contains(pet)) return false;
            debugDrawPets.Remove(pet);
            return true;
        }
    }
}