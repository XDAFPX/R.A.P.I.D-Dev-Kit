using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Archon.SwissArmyLib.Utils.Editor;
using BandoWare.GameplayTags;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Modifiers.Pegs;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Components;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.GlobalState.Events;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.ViewModel;
using DAFP.TOOLS.Injection;
using FluentResults;
using ModestTree;
using PixelRouge.Inspector;
using UGizmo;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;
using NRandom;
using PixelRouge.CsharpExtensionMethods;
using PixelRouge.Inspector.Extensions;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;

namespace DAFP.TOOLS.ECS
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public abstract class Entity : MonoBehaviour, IEntity, IRandomizeable, ISavable,
        IOwnedBy<IDebugDrawable>, IEntityLogic, IResetable
    {
        // Serialized Fields
        [ReadOnly] [SerializeField] private string id;


        [SerializeField] private SerializableInterface<IHaveGameplayTag> Tag;
        [SerializeField] internal SerializableInterface<IThinker> _brains;
        [SerializeField] private StatContainer _stats;


        //-- Implementations

        public GameplayTagContainer GameplayTag
        {
            get => Tag?.Value == null ? GameplayTagContainer.Empty : Tag.Value.GameplayTag;
            set => Tag = new SerializableInterface<IHaveGameplayTag>(value);
        }


        private IStatContainer dummyStats = new DummyStatContainer();

        public IStatContainer Stats
        {
            get
            {
                if (_stats == null)
                    return dummyStats;
                return _stats;
            }
            set
            {
                switch (value)
                {
                    case StatContainer val:
                        _stats = val;
                        break;
                    case null:
                        _stats = null;
                        break;
                    // Any other IStatContainer impl won't be set cuz I'm tired
                    default:
                        break;
                }
            }
        }

        private readonly IThinker dummyBrain = new DummyThinker();

        public IThinker Brains
        {
            get => _brains?.Value ?? dummyBrain;
            // set handled elsewhere
            // {
            //     if (_brains.Value == value)
            //         return;
            //     end_brains(_brains.Value);
            //     Adam.Destroy(_brains.Value).Forget();
            //     _brains.Value = value;
            //     start_brains(_brains.Value);
            // }
        }

        void IEntityLogic.SetThinker(IThinker thinker)
        {
            _brains.Value = thinker;
        }

        ITickable IEntityLogic.ThinkerTicker
        {
            get => brainTicker;
            set => brainTicker = value;
        }

        private ITickable brainTicker;


        [SerializeField] private List<SerializableInterface<IViewModel>> view = new();

        public virtual ICollection<IViewModel> View
        {
            get
            {
                Debug.Assert(realView != null);
                if (!realView.IsEmpty()) return realView;

                var _backup = SetupView();
                var _viewModels = _backup as IViewModel[] ?? _backup.ToArray();
                if (_viewModels.IsNullOrEmpty())
                    _viewModels = new EmptyView().ToEnumerable().ToArray<IViewModel>();
                foreach (var _viewModel in _viewModels)
                {
                    realView.Add(_viewModel);
                }

                return realView;
            }
        }

        private DelegateSet<IViewModel> realView;

        // Dependencies
        [Inject] protected DiContainer Injector;
        [Inject] protected World World;
        [Inject] protected Adam Adam;
        [Inject] protected ISaveSystem SaveSystem;
        [Inject] protected IRandom Rng;
        [Inject] protected IAudioSystem AudioSystem;

        [Inject] public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem { get; }


        // Components & Memory
        public BlackBoard Memory { get; private set; }

        // Pets & Ownership

        private List<IDebugDrawable> owners;
        private List<IViewModel> viewModels = new();
        protected List<IStatBase> OwnedStats = new();
        protected List<IStatModifierBase> OwnedModifiers = new();
        private List<PegModifier> ownedPegs = new();
        protected List<IEntityAccessory> Accessories = new();
        protected IReadOnlyList<IEntityComponent> EntComponents = new List<IEntityComponent>();

        public List<IEntity> Children { get; } = new();

        private ISet<IDebugDrawable> debugDrawablePets = new HashSet<IDebugDrawable>();

        //-------------------------------------------
        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => debugDrawablePets;
        IEnumerable<IViewModel> IOwnerOf<IViewModel>.Pets => viewModels;

        IEnumerable<IStatBase> IOwnerOf<IStatBase>.Pets => Stats.All();

        IEnumerable<IStatModifierBase> IOwnerOf<IStatModifierBase>.Pets => OwnedModifiers;

        IEnumerable<PegModifier> IOwnerOf<PegModifier>.Pets => ownedPegs;

        public IEnumerable<IEntityAccessory> Pets => Accessories;

        private void refresh_component_cache()
        {
            EntComponents = this.Components().OfType<IEntityComponent>().ToList();
        }

        public void AddPet(IEntityAccessory pet)
        {
            GameUtils.AddPet(pet, Accessories);
        }

        public bool RemovePet(IEntityAccessory pet)
        {
            return GameUtils.RemovePet(pet, Accessories);
        }

        public void AddPet(PegModifier pet)
        {
            GameUtils.AddPet(pet, ownedPegs);
        }

        public bool RemovePet(PegModifier pet)
        {
            return GameUtils.RemovePet(pet, ownedPegs);
        }

        public void AddPet(IStatModifierBase pet)
        {
            GameUtils.AddPet(pet, OwnedModifiers);
        }

        public bool RemovePet(IStatModifierBase pet)
        {
            return GameUtils.RemovePet(pet, OwnedModifiers);
        }

        public void AddPet(IStatBase pet)
        {
            GameUtils.AddPet(pet, OwnedStats);
        }

        public bool RemovePet(IStatBase pet)
        {
            return GameUtils.RemovePet(pet, OwnedStats);
        }

        public void AddPet(IViewModel pet)
        {
            GameUtils.AddPet(pet, viewModels);
        }

        public bool RemovePet(IViewModel pet)
        {
            return GameUtils.RemovePet(pet, viewModels);
        }

        public List<IEntity> Owners { get; } = new();

        private IDebugDrawable owner;

        public IDebugDrawable GetCurrentOwner()
        {
            return owner;
        }

        public void ChangeOwner(IDebugDrawable newOwner)
        {
            owner = newOwner;
        }

        void IOwnerOf<IDebugDrawable>.AddPet(IDebugDrawable pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return;
            debugDrawablePets.Add(pet);
        }

        bool IOwnerOf<IDebugDrawable>.RemovePet(IDebugDrawable pet)
        {
            if (pet == null || ReferenceEquals(pet, this)) return false;
            return debugDrawablePets.Remove(pet);
        }


        // Public Properties & Events
        public string ID => id;

        public virtual string Name
        {
            get => name;
            set => name = value;
        }

        private Bounds? cachedBounds;
        private int cachedFrame;

        public Bounds CachedBounds
        {
            get
            {
                if (!cachedBounds.HasValue) return Bounds;
                var _bb = cachedBounds.Value;
                _bb.center += transform.position;
                return _bb;
            }
        }

        public virtual Bounds Bounds
        {
            get
            {
                if (cachedBounds.HasValue && cachedFrame == Time.frameCount)
                {
                    var _bb = cachedBounds.Value;
                    _bb.center += transform.position;
                    return _bb;
                }

                var _bb2 = CalculateBounds();
                var _localbb = _bb2;
                _localbb.center -= transform.position;
                cachedBounds = _localbb;
                cachedFrame = Time.frameCount;
                return _bb2;
            }
        }


        public virtual IVector EyeVector => (V3)transform.forward;
        public bool HasInitialized { get; private set; }

        internal bool Instantiated => TryGetComponent<CreationInfoContainer>(out var _component);

        // Abstract Members

        public abstract IEnumerable<IViewModel> SetupView();
        public abstract ITicker EntityTicker { get; }
        protected abstract void InitializeInternal();
        protected abstract void TickInternal();


        // Configuration Methods

        [Button("Generate new ID", EButtonMode.EditorOnly)]
        internal void GenNewID()
        {
            id = Guid.NewGuid().ToString();
        }

        [Button("Regenerate All Stats", EButtonMode.EditorOnly)]
        internal void FixStats()
        {
            if (Stats == null)
                return;
            // Stats = ScriptableObject.Instantiate<StatContainer>(Stats);
            assemble_list_additional_of_code_sources(out var _ads);

            StatInjector.FixStats(this, _ads);
        }

        private void inject_stats()
        {
            if (Stats == null)
                return;
            // Stats = ScriptableObject.Instantiate<StatContainer>(Stats);
            assemble_list_additional_of_code_sources(out var _ads);

            StatInjector.InjectStats(this, _ads);
        }

        // Initialization & Lifecycle
        internal void Reset()
        {
            GenNewID();
            FixStats();
            HasInitialized = false;
            var _backup = SetupView();
            var _viewModels = _backup as IViewModel[] ?? _backup.ToArray();
            if (_viewModels.IsNullOrEmpty())
                _viewModels = new EmptyView().ToEnumerable().ToArray<IViewModel>();
            var _inter = _viewModels.Select((model => new SerializableInterface<IViewModel>(model)));
            view = _inter.ToList();
        }

        protected virtual void OnValidate()
        {
            if (!gameObject.activeInHierarchy)
                return;
            try
            {
                assemble_list_additional_of_code_sources(out var _ads);
                StatInjector.InjectStats(this, _ads);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[{nameof(StatInjector)}] Bad stats detected at object '{name}' regenerating... ");
                FixStats();
            }

            // Editor-time: check that every component on this GameObject has its GetComponentCache dependencies satisfied
            try
            {
                // Check this Entity first
                GetComponentCacheInitializer.HasAllDependencies(this, new Component[] { this });
                // Then check all other MonoBehaviours on the same GameObject
                var behaviours = GetComponents<MonoBehaviour>();
                foreach (var mb in behaviours)
                {
                    if (mb == null) continue;
                    if (ReferenceEquals(mb, this)) continue;
                    GetComponentCacheInitializer.HasAllDependencies(mb);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entity] Dependency validation threw on '{name}': {ex.Message}");
            }

            if (!view.IsNullOrEmpty()) return;
            var _backup = SetupView();
            var _viewModels = _backup as IViewModel[] ?? _backup.ToArray();
            if (_viewModels.IsNullOrEmpty())
                _viewModels = new EmptyView().ToEnumerable().ToArray<IViewModel>();
            var _inter = _viewModels.Select((model => new SerializableInterface<IViewModel>(model)));
            view = _inter.ToList();
        }

        private void wake_up(World world)
        {
            // if (world == null || world.IsRegistered(this) || HasInitialized)
            //     return;

            Memory = new BlackBoard(this);
            refresh_component_cache();


            foreach (var _comp in EntComponents)
                _comp.Register(this);

            if (string.IsNullOrEmpty(id) || Instantiated)
                GenNewID();

            // foreach (var _ownedBy in detect_child_entities().Cast<IOwnedBy<IEntity>>())
            // {
            //     if (_ownedBy.GetCurrentOwner() != null) continue;
            //     _ownedBy.ChangeOwner(this);
            // } -- moved to bootstrap 
            inject_stats();
            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this, gameObject, this.ToEnumerable().ToArray<Component>());
        }


        void IEntityLogic.Initialize()
        {
            if (HasInitialized)
                return;
            wake_up(World);
            trigger_randomizers();
            initialize_tag();
            setup_entity_stats();

            foreach (var _comp in EntComponents)
                _comp.Initialize();

            initialize_view();
            InitializeInternal();

            // initialize_debug(); -- fuck this I should move this to somewhere else TODO
            HasInitialized = true;
            // DebugSystem.Log(World, $"{Name} entity is initialized");
        }

        private void trigger_randomizers()
        {
            foreach (var _randomizer in GetComponents<IRandomizer>())
            {
                _randomizer.Randomize(Rng);
            }
        }

        private void initialize_view()
        {
            realView = new DelegateSet<IViewModel>((() => view.ToValues()),
                model =>
                {
                    view.Add(new SerializableInterface<IViewModel>(model));
                    return true;
                },
                (model =>
                {
                    var m = view.FirstOrDefault((viewModel => model == viewModel.Value));
                    if (m != null) view.Remove(m);
                    return m != null;
                }),
                (model => view.ToValues().Contains(model)), (() => view.Clear()));


            IEnumerable<IViewModel> _finalView = View;
            register();

            void register()
            {
                _finalView.ThrowIfNull(nameof(view));

                var _viewModels = _finalView as IViewModel[] ?? _finalView.ToArray();
                _viewModels.ForEach((model => model.InitOwner(this)));


                if (!TryGetComponent(out IHurtBoxController<IEntity> _controller))
                    return;
                _viewModels.Select(v => v.GetHurtGroup(this))
                    .Where(group => group != null)
                    .ForEach(_controller.AddPet);
            }
        }

        private void setup_entity_stats()
        {
            if (_stats == null)
                return;
            Stats = ScriptableObject.Instantiate(_stats);
            assemble_list_additional_of_code_sources(out var _additions);
            StatInjector.InjectStats(this, _additions);
            Stats.Construct(this);
        }

        private void assemble_list_additional_of_code_sources(out object[] additions)
        {
            IEnumerable<object> comps = GetComponents<MonoBehaviour>();

            var brains = _brains?.Value;
            if (brains != null)
                comps = comps.Concat(new[] { brains });
            additions = comps.ToArray();
        }


        public void Tick()
        {
            if (!HasInitialized)
                return;
            tick_stats();
            tick_components();
            TickInternal();
        }


        private void tick_stats()
        {
            Stats?.Tick();
        }


        private void tick_components()
        {
            foreach (var _comp in EntComponents)
            {
                if (_comp is IViewModel)
                    continue;
                if (_comp.EntityComponentTicker == EntityTicker)
                    _comp.Tick();
            }
        }
        


        protected virtual Bounds CalculateBounds()
        {
            return GameUtils.CalculateCombinedBounds(this);
        }


        // private void initialize_brains(IThinker thinker) // move the DI logic to the creation process
        // { move to some brain manager idk
        //     if (thinker == null)
        //         return;
        //     clone_or_assign_brain(thinker);
        //     if (!thinker.DIInjected)
        //     {
        //         Injector.Inject(Brains);
        //         foreach (var _ownable in Brains.AllPets())
        //         {
        //             if (_ownable is IThinker _brain)
        //             {
        //                 Injector.Inject(_brain);
        //             }
        //         }
        //     }
        //
        //     Brains.TryInitialize(this);
        // }

        private void initialize_tag()
        {
            if (Tag?.Value is ScriptableObject _obj)
            {
                Tag = new SerializableInterface<IHaveGameplayTag>((IHaveGameplayTag)ScriptableObject.Instantiate(_obj));
            }
        }

        [Inject(Optional = true)] private EntityDebugDrawer.BoundingBoxDrawer bounding_box_drawer;
        [Inject(Optional = true)] private EntityDebugDrawer.PositionDrawer positionDrawer;
        [Inject(Optional = true)] private EntityDebugDrawer.HealthDrawer healthDrawer;

        protected virtual IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return new EntityDebugDrawer[] { bounding_box_drawer, positionDrawer, healthDrawer }.ClearOfNulls();
        }

        private void initialize_debug()
        {
            var _drawers = SetupDebugDrawers().ToList();
            foreach (var _entityComponent in EntComponents)
                _drawers = _drawers.Union(_entityComponent.SetupDebugDrawers()).ToList();


            foreach (var _drawer in _drawers)
            {
                ((IOwnerOf<IDebugDrawable>)this).AddPet(_drawer);
            }

            var debug = ((IOwnerOf<IDebugDrawable>)this).Pets.ToList();
            foreach (var _ownable in debug)
            {
                _ownable.ChangeOwner(this);
                if (_ownable is IDebugDrawer _drawer) _drawer.InitilizeDebugDrawer(DebugSystem);
            }

            DebugSystem.AddPet(this);
        }


        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }

        public World GetWorld()
        {
            return World;
        }


        // Saving & Loading
        public virtual ISaveData Save()
        {
            return new GenericSaveData(new());
        }

        public virtual void Load(ISaveData saveData)
        {
        }

        // Randomization
        public void Randomize(IRandom rng, float margin01)
        {
            foreach (var _comp in EntComponents)
                if (_comp is IRandomizeable _rnd)
                    _rnd.Randomize(rng, margin01);
        }


        public static implicit operator GameObject(Entity ent)
        {
            return ent.GetWorldRepresentation();
        }


        // public void Dispose() --migrate everything to the destructor
        // {
        //     // DisposeAdditional();
        //     DeInitializeBrains(Brains);
        //     cleanup_brains();
        //     DebugSystem.RemovePet(this);
        //     GameEventsBus.UnSubscribe(this);
        //     World.RemoveEntity(this);
        // }

        // protected virtual void DisposeAdditional()
        // {
        // }


        private void OnDrawGizmos()
        {
            //DO NOT OVERRIDE PLEASSSEEE
        }


        //Enable && Disable impl
        public bool Enabled => gameObject.activeInHierarchy;

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        void IResetable.ResetToDefault()
        {
            HasInitialized = false;
            Stats.ResetToDefault();
            EntComponents.OfType<IResetable>().ForEach((component => component.ResetToDefault()));
        }

        //--Helpers
        private readonly ConcurrentDictionary<(Type, string), FieldInfo> fieldCache = new();


        public IEnumerable<object> AbsolutePets => Children.Union<object>(debugDrawablePets).Union(Accessories)
            .Union(OwnedModifiers).Union(ownedPegs).Union(viewModels);
    }
}