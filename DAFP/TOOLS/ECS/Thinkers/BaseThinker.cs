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
using TNRD;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public interface IThinker : IThinkerHierarchy, IPetOf<IDebugDrawable>, IDebugDrawable,
        IOwnerOf<IThinker>,
        IPetOf<IThinker>, IOwnedBy<IThinker>, ISubscriber
    {
        bool DIInjected { get; }
        bool HasInitialized { get; }
        void Initialize(IEntity host);
        void Tick(IEntity host, ITickerBase ticker);
        void Dispose(IEntity host);
    }

// Hierarchical structure
    public interface IThinkerHierarchy
    {
        IThinker[] Children { get; }
    }

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
        [SerializeField] private SerializableInterface<IThinker>[] ChildThinkers;

        private IThinker[] childCache
        {
            get
            {
                _childCache ??= ChildThinkers.Select((@interface => @interface.Value)).ToArray();
                return _childCache;
            }
        }

        private IThinker[] _childCache;
        private List<IThinker> owners = new();

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
            if (DebugPets == null || DebugPets.IsEmpty())
                return;
            DebugPets.Clear();
            DebugSystem.RemovePet(this);
        }

        private void init_debug_drawers(IEnumerable<IDebugDrawer> pets)
        {
            if (pets == null)
                return;
            var _debugDrawers = pets as IDebugDrawer[] ?? pets.ToArray();
            if (_debugDrawers.IsEmpty())
                return;
            DebugPets.UnionWith(_debugDrawers);
            var petsSnapshot = DebugPets.ToArray();

            foreach (var _ownable in petsSnapshot)
            {
                _ownable.ChangeOwner(this);
                if (_ownable is IDebugDrawer _drawer)
                    _drawer.Initilize(DebugSystem);
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
                ((IPetOf<IThinker>)_child.Value).ChangeOwner(this);
                _child.Value.Initialize(host);
            }
        }

        private readonly ISet<IOwnedBy<IDebugDrawable>> DebugPets = new HashSet<IOwnedBy<IDebugDrawable>>();
        public List<IDebugDrawable> Owners { get; } = new();

        IEnumerable<IOwnedBy<IDebugDrawable>> IOwnerOf<IDebugDrawable>.Pets => DebugPets;
        void IOwnerOf<IDebugDrawable>.AddPet(IOwnedBy<IDebugDrawable> pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return;
            DebugPets.Add(pet);
        }
        bool IOwnerOf<IDebugDrawable>.RemovePet(IOwnedBy<IDebugDrawable> pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return false;
            return DebugPets.Remove(pet);
        }

        IEnumerable<IOwnedBy<IThinker>> IOwnerOf<IThinker>.Pets => childCache.Cast<IOwnedBy<IThinker>>();
        void IOwnerOf<IThinker>.AddPet(IOwnedBy<IThinker> pet) { /* no-op: children are defined via ChildThinkers */ }
        bool IOwnerOf<IThinker>.RemovePet(IOwnedBy<IThinker> pet) { return false; }

        List<IThinker> IPetOf<IThinker>.Owners => owners;


        // -- Generic implementations
        public IThinker[] Children => childCache;
    }
}