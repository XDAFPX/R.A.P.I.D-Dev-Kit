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
    public interface IThinker : IThinkerHierarchy, IPet<IDebugDrawable>, IDebugDrawable,
        IOwner<IThinker>,
        IPet<IThinker>, IOwnable<IThinker>, ISubscriber
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
            if (Pets == null || Pets.IsEmpty())
                return;
            Pets.Clear();
            DebugSystem.RemovePet(this);
        }

        private void init_debug_drawers(IEnumerable<IDebugDrawer> pets)
        {
            if (pets == null)
                return;
            var _debugDrawers = pets as IDebugDrawer[] ?? pets.ToArray();
            if (_debugDrawers.IsEmpty())
                return;
            Pets.UnionWith(_debugDrawers);
            var petsSnapshot = Pets.ToArray();

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
                ((IPet<IThinker>)_child.Value).ChangeOwner(this);
                _child.Value.Initialize(host);
            }
        }

        public ISet<IOwnable<IDebugDrawable>> Pets { get; } = new HashSet<IOwnable<IDebugDrawable>>();
        public List<IDebugDrawable> Owners { get; } = new();

        ISet<IOwnable<IThinker>> IOwner<IThinker>.Pets => childCache.Cast<IOwnable<IThinker>>().ToHashSet();

        List<IThinker> IPet<IThinker>.Owners => owners;


        // -- Generic implementations
        public IThinker[] Children => childCache;
    }
}