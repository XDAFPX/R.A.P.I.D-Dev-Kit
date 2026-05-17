using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archon.SwissArmyLib.Utils.Editor;
using BandoWare.GameplayTags;
using DAFP.GAME.Assets;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Modifiers.Pegs;
using DAFP.TOOLS.ECS.BuiltIn;
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
using ModestTree;
using PixelRouge.Inspector;
using UGizmo;
using UnityEngine;
using UnityEventBus;
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
    public abstract class Entity : MonoBehaviour, IEntity, IDisposable, IRandomizeable, ISavable,
        IListener<OnGameStateChanged>, IListener<OnSaveMadeOrLoaded>, IOwnedBy<IDebugDrawable>
    {
        // Serialized Fields
        [ReadOnly] [SerializeField] private string id;


        [SerializeField] private SerializableInterface<IHaveGameplayTag> Tag;
        [SerializeField] private SerializableInterface<IThinker> _brains;
        [SerializeField] private StatContainer _stats;


        //-- Implementations


        public IStatContainer Stats
        {
            get
            {
                if (_stats == null)
                    return new DummyStatContainer();
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

        public IThinker Brains
        {
            get => _brains?.Value;
            set
            {
                if (_brains != null) _brains.Value = value;
            }
        }


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
        [Inject] protected ISaveSystem SaveSystem;
        [Inject] protected IRandom RandomSys;
        [Inject] protected IAssetManager AssetManager;
        [Inject] protected IAudioSystem AudioSystem;

        [Inject] public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem { get; }
        [Inject(Id = IVideoGame.GAME_BUS_NAME)] protected IEventBus GameEventsBus;

        // Components & Memory
        public Dictionary<Type, IEntityComponent> Components { get; } = new();
        public BlackBoard Memory { get; private set; }

        // Pets & Ownership

        private List<IDebugDrawable> owners;
        private List<IViewModel> viewModels = new();
        protected List<IStatBase> OwnedStats = new();
        protected List<IStatModifierBase> OwnedModifiers = new();
        private List<PegModifier> ownedPegs = new();
        protected List<IEntityAccessory> Accessories = new();

        public List<IEntity> Children { get; } = new();

        private ISet<IDebugDrawable> debugDrawablePets = new HashSet<IDebugDrawable>();

        //-------------------------------------------
        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => debugDrawablePets;
        IEnumerable<IViewModel> IOwnerOf<IViewModel>.Pets => viewModels;

        IEnumerable<IStatBase> IOwnerOf<IStatBase>.Pets => OwnedStats;

        IEnumerable<IStatModifierBase> IOwnerOf<IStatModifierBase>.Pets => OwnedModifiers;

        IEnumerable<PegModifier> IOwnerOf<PegModifier>.Pets => ownedPegs;

        public IEnumerable<IEntityAccessory> Pets => Accessories;

        public void AddPet(IEntityAccessory pet)
        {
            GameUtils.AddPet(pet, ref Accessories);
        }

        public bool RemovePet(IEntityAccessory pet)
        {
            return GameUtils.RemovePet(pet, ref Accessories);
        }

        public void AddPet(PegModifier pet)
        {
            GameUtils.AddPet(pet, ref ownedPegs);
        }

        public bool RemovePet(PegModifier pet)
        {
            return GameUtils.RemovePet(pet, ref ownedPegs);
        }

        public void AddPet(IStatModifierBase pet)
        {
            GameUtils.AddPet(pet, ref OwnedModifiers);
        }

        public bool RemovePet(IStatModifierBase pet)
        {
            return GameUtils.RemovePet(pet, ref OwnedModifiers);
        }

        public void AddPet(IStatBase pet)
        {
            GameUtils.AddPet(pet, ref OwnedStats);
        }

        public bool RemovePet(IStatBase pet)
        {
            return GameUtils.RemovePet(pet, ref OwnedStats);
        }

        public void AddPet(IViewModel pet)
        {
            GameUtils.AddPet(pet, ref viewModels);
        }

        public bool RemovePet(IViewModel pet)
        {
            return GameUtils.RemovePet(pet, ref viewModels);
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
        public bool HasInitialized { get; set; }
        public event IEntity.TickCallBack OnTick;

        // Abstract Members

        public abstract IEnumerable<IViewModel> SetupView();
        public abstract ITicker<IEntity> EntityTicker { get; }
        protected abstract void InitializeInternal();
        protected abstract void TickInternal();

        // Unity Messages

        private bool isInstantiated;

        public void FlagAsInstantiated()
        {
            GenNewID();
            isInstantiated = true;
        }

        private void kick_start()
        {
            if (World == null)
                return;
            if (World.IsRegistered(this))
                return;
            boot_strap(World);
        }

        private void Awake()
        {
            if (!isInstantiated)
            {
                kick_start();
            }
        }

        private void Update()
        {
            if (isInstantiated && !HasInitialized)
            {
                kick_start();
            }
        }


        // Configuration Methods

        [Button("Generate new ID", EButtonMode.EditorOnly)]
        public void GenNewID()
        {
            id = Guid.NewGuid().ToString();
        }

        [Button("Regenerate All Stats", EButtonMode.EditorOnly)]
        public void SetupStats()
        {
            if (Stats == null)
                return;
            // Stats = ScriptableObject.Instantiate<StatContainer>(Stats);
            assemble_list_additional_of_code_sources(out var _ads);

            StatInjector.FixStats(this, _ads);
        }


        // Initialization & Lifecycle
        public void Reset()
        {
            GenNewID();
            SetupStats();
            var _backup = SetupView();
            var _viewModels = _backup as IViewModel[] ?? _backup.ToArray();
            if (_viewModels.IsNullOrEmpty())
                _viewModels = new EmptyView().ToEnumerable().ToArray<IViewModel>();
            var _inter = _viewModels.Select((model => new SerializableInterface<IViewModel>(model)));
            view = _inter.ToList();
        }

        private void OnValidate()
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
                SetupStats();
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

        private void boot_strap(World world)
        {
            Memory = new BlackBoard(this);


            if (string.IsNullOrEmpty(id))
                GenNewID();

            foreach (var _ownedBy in detect_child_entities().Cast<IOwnedBy<IEntity>>())
            {
                if (_ownedBy.GetCurrentOwner() != null) continue;
                _ownedBy.ChangeOwner(this);
            }


            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this, gameObject, this.ToEnumerable().ToArray<Component>());

            gather_components();


            world.RegisterEntity(this, EntityTicker);
        }


        public void Initialize()
        {
            initialize_tag();

            setup_entity_stats();

            // SaveSystem.Bus.Subscribe(this);
            GameEventsBus.Subscribe(this);

            foreach (var _comp in Components.Values)
                _comp.Initialize();


            initialize_view();
            InitializeInternal();
            InitializeBrains(Brains);
            initialize_debug();
            HasInitialized = true;

            DebugSystem.Log(World, $"{Name} entity is initialized");


            initialize_children();
        }

        private void initialize_children()
        {
            foreach (var _entity in Children)
            {
                if (_entity.HasInitialized)
                    continue;
                _entity.Initialize();
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
            tick_stats();
            tick_components();

            TickInternal();

            tick_brains(Brains);


            OnTick?.Invoke(this);
        }


        private void tick_stats()
        {
            Stats?.Tick();
        }

        private void tick_brains(IThinker brains)
        {
            brains?.Tick(this, EntityTicker);
        }


        private void tick_components()
        {
            foreach (var _comp in Components.Values)
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

        // Component Management
        private void gather_components()
        {
            if (Components == null)
                return;
            Components.Clear();
            foreach (var _comp in gameObject.GetComponents<IEntityComponent>())
                AddEntComponent(_comp);
        }

        public void AddEntComponent(IEntityComponent component)
        {
            if (component is IStatBase _stat && _stat.SyncToBlackBoard && Memory != null)
            {
                Memory.Set(_stat.Name, _stat.GetAbsoluteValue());
                _stat.OnUpdateValue += OnUpdateStat;
            }

            Components[component.GetType()] = component;
            component.Register(this);
        }

        private void OnUpdateStat(IStatBase stat)
        {
            if (stat.SyncToBlackBoard)
                Memory.Set(stat.Name, stat.GetAbsoluteValue());
        }

        public void DeInitializeBrains(IThinker thinker)
        {
            if (thinker == null)
                return;
            thinker.TryDeInitialize(this);
            Brains = null;
        }

        public void InitializeBrains(IThinker thinker)
        {
            if (thinker == null)
                return;
            clone_or_assign_brain(thinker);
            if (thinker.DIInjected)
            {
                Injector.Inject(Brains);
                foreach (var _ownable in Brains.AllPets())
                {
                    if (_ownable is IThinker _brain)
                    {
                        Injector.Inject(_brain);
                    }
                }
            }

            Brains.TryInitialize(this);
        }

        private void initialize_tag()
        {
            if (Tag?.Value is ScriptableObject _obj)
            {
                Tag = new SerializableInterface<IHaveGameplayTag>((IHaveGameplayTag)ScriptableObject.Instantiate(_obj));
            }
        }

        private void clone_or_assign_brain(IThinker thinker)
        {
            Brains = thinker;
            if (Brains is not BaseThinker || thinker is not BaseThinker _t )
                return;
#if UNITY_EDITOR

            if (Brains is BaseThinker { EditMode: false } )
                Brains = _t.DeepClone();
            else
            {
                Brains = thinker;
            }
#else
                Brains = _t.DeepClone();
#endif
        }

        protected virtual IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return new IDebugDrawer[]
            {
                new EntityDebugDrawer.PositionDrawer(),
                new EntityDebugDrawer.HealthDrawer(), new EntityDebugDrawer.BoundingBoxDrawer()
            };
        }

        private void initialize_debug()
        {
            var _drawers = SetupDebugDrawers().ToList();
            foreach (var _entityComponent in Components)
                _drawers = _drawers.Union(_entityComponent.Value.SetupDebugDrawers()).ToList();


            foreach (var _drawer in _drawers)
            {
                ((IOwnerOf<IDebugDrawable>)this).AddPet(_drawer);
            }

            foreach (var _ownable in ((IOwnerOf<IDebugDrawable>)this).Pets)
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

        public IEventBus Bus { get; } = new EntityBus();

        // Saving & Loading
        public virtual ISaveData Save()
        {
            return new GenericSaveData();
        }

        public virtual void Load(ISaveData saveData)
        {
        }

        // Randomization
        public void Randomize(IRandom rng, float margin01)
        {
            foreach (var _comp in Components.Values)
                if (_comp is IRandomizeable _rnd)
                    _rnd.Randomize(rng, margin01);
        }


        // Event Reactions
        public virtual void React(in OnGameStateChanged e)
        {
        }

        public virtual void React(in OnSaveMadeOrLoaded e)
        {
        }

        public static implicit operator GameObject(Entity ent)
        {
            return ent.GetWorldRepresentation();
        }

        public virtual void Remove(EntityRemovalReason removalReason)
        {
            Dispose();
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            OnDispose();
        }

        private void cleanup_brains()
        {
            if (Brains is not BaseThinker)
                return;
#if UNITY_EDITOR
            if (Brains != null && Brains is BaseThinker { EditMode: false } _thinker) _thinker.DeepDestroy();
#else
#endif
            Brains = null;
        }

        protected virtual void OnDispose()
        {
            DeInitializeBrains(Brains);
            cleanup_brains();
            DebugSystem.RemovePet(this);
            GameEventsBus.UnSubscribe(this);
            World.RemoveEntity(this);
        }

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

        //--Helpers
        private readonly ConcurrentDictionary<(Type, string), FieldInfo> fieldCache = new();

        public void BroadcastEvent<T>(T @event) where T : struct
        {
            Bus.Send(@event);
            GameEventsBus.Send(@event);
        }


        public class EntityBus : EventBusImpl, IEventBus
        {
        }


        public GameplayTagContainer GameplayTag
        {
            get { return Tag?.Value == null ? GameplayTagContainer.Empty : Tag.Value.GameplayTag; }
        }


        public IEnumerable<object> AbsolutePets => Children.Union<object>(debugDrawablePets).Union(Accessories)
            .Union(OwnedModifiers).Union(ownedPegs).Union(viewModels);


        private IEnumerable<IEntity> detect_child_entities()
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<IEntity>(out var entity))
                {
                    yield return entity;
                }
                else
                {
                    foreach (var nested in detect_child_entities_recursive(child))
                        yield return nested;
                }
            }
        }

        private IEnumerable<IEntity> detect_child_entities_recursive(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.TryGetComponent<IEntity>(out var entity))
                    yield return entity;
                else
                    foreach (var nested in detect_child_entities_recursive(child))
                        yield return nested;
            }
        }

        public static IEntity find_nearest_ent_up_parent_tree(Transform start)
        {
            Transform current = start.parent;

            while (current != null)
            {
                if (current.TryGetComponent<IEntity>(out var entity))
                    return entity;

                current = current.parent;
            }

            return null;
        }

    }
}