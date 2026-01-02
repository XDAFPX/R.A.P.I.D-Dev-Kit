using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using BandoWare.GameplayTags;
using DAFP.GAME.Assets;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.ViewModel;
using PixelRouge.Inspector;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using UnityGetComponentCache;
using Zenject;
using NRandom;
using TNRD;

namespace DAFP.TOOLS.ECS
{
    public abstract class Entity : MonoBehaviour, IEntity, IDisposable, IEntityPet, IRandomizeable, ISavable,
        IListener<IGlobalStateChanged>, IListener<OnSaveMadeOrLoaded>, IPet<IDebugDrawable>
    {
        // Serialized Fields
        [ReadOnly] [SerializeField] private string id;

        [SerializeField] private SerializableInterface<IHaveGameplayTag> Tag;
        [SerializeField] private SerializableInterface<IThinker> _brains;
        [field: SerializeField] public StatContainer Stats { get; private set; }

        public IThinker Brains
        {
            get => _brains.Value;
            set => _brains.Value = value;
        }

        [InspectorName("RandomizingFrom0To100")] [Range(0, 100)] [SerializeField]
        private float Variety = 10;


        // Dependencies
        [Inject] protected DiContainer Injector;
        [Inject] protected World World;
        [Inject] protected ISaveSystem SaveSystem;
        [Inject] protected IRandom RandomSys;
        [Inject] protected IAssetManager AssetManager;

        [Inject] public IDebugSys<IGlobalGizmos, IMessenger> DebugSystem { get; }
        [Inject(Id = "GlobalStateBus")] protected IEventBus GlobalStateBus;
        [Inject(Id = "GlobalGameEventsBus")] protected IEventBus GameEventsBus;

        // Components & Memory
        public Dictionary<Type, IEntityComponent> Components { get; } = new();
        public BlackBoard Memory { get; set; }

        // Pets & Ownership
        protected readonly ISet<IOwnable<IEntity>> Pets = new HashSet<IOwnable<IEntity>>();
        ISet<IOwnable<IEntity>> IOwner<IEntity>.Pets => Pets;
        ISet<IOwnable<IDebugDrawable>> IOwner<IDebugDrawable>.Pets { get; } = new HashSet<IOwnable<IDebugDrawable>>();
        public List<IEntity> Owners { get; } = new();

        // Public Properties & Events
        public string ID => id;

        public virtual string Name
        {
            get => GetType().FullName + $": ({gameObject.name})";
            set { }
        }

        public virtual Bounds Bounds { private set; get; }
        public virtual IVectorBase EyeVector => (V3)transform.forward;
        private Bounds Localbounds { set; get; }
        public virtual NonEmptyList<IViewModel> View { private set; get; }
        public bool HasInitialized { get; set; }
        public event IEntity.TickCallBack OnTick;

        // Abstract Members
        public abstract NonEmptyList<IViewModel> SetupView();
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
            Initialize();
            OnStart();
        }

        private void Start()
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

        [Button("Fix stats", EButtonMode.EditorOnly)]
        public void SetupStats()
        {
            StatInjector.InjectAndValidateStats(this);
        }




        // Initialization & Lifecycle
        public void Reset()
        {
            GenNewID();
        }

        public void Initialize()
        {
            Memory = new BlackBoard(null, this);
            if (string.IsNullOrEmpty(id))
                GenNewID();
            initialize_tag();
            World.RegisterEntity(this, EntityTicker);
            gather_components();

            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);
            SetupStats();

            SaveSystem.Bus.Subscribe(this);
            GlobalStateBus.Subscribe(this);
            GameEventsBus.Subscribe(this);

            foreach (var _comp in Components.Values)
                _comp.Initialize();

            View = SetupView();
            Bounds = CalculateBounds();
            InitializeInternal();
            InitializeBrains(Brains);
            initialize_debug();
            HasInitialized = true;
        }


        public void OnStart()
        {
            if (Variety != 0)
                Randomize(RandomSys, Variety / 100f);

            foreach (var _comp in Components.Values)
                _comp.OnStartInternal();
        }

        public void Tick()
        {
            tick_components();

            TickInternal();

            tick_brains(Brains);

            tick_bounds();

            OnTick?.Invoke(this);
        }

        private void tick_brains(IThinker brains)
        {
            brains?.Tick(this, EntityTicker);
        }

        private void tick_bounds()
        {
            if (BoundsRefreshRate < 0)
                return;
            //Recalculate bounds every couple of frames
            if (bounds_calc >= BoundsRefreshRate)
            {
                Localbounds = CalculateBounds();
                bounds_calc = 0;
            }
            else
            {
                bounds_calc++;
            }

            var _bvb = Localbounds;
            _bvb.center += transform.position;
            Bounds = _bvb;
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

        private int bounds_calc = 0;
        private List<IDebugDrawable> owners;
        protected virtual int BoundsRefreshRate => 30;
        public void RecalculateBounds() => CalculateBounds();

        protected virtual Bounds CalculateBounds()
        {
            return Utils.CalculateCombinedBounds(this);
        }

        // Component Management
        private void gather_components()
        {
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
                foreach (var _ownable in ((IOwner<IThinker>)Brains).EnumeratePetsDeep())
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
            if (Tag.Value is ScriptableObject _obj)
            {
                Tag = new SerializableInterface<IHaveGameplayTag>((IHaveGameplayTag)ScriptableObject.Instantiate(_obj));
            }
        }

        private void clone_or_assign_brain(IThinker thinker)
        {
            Brains = thinker;
            if (Brains is not BaseThinker || thinker is not BaseThinker)
                return;
#if UNITY_EDITOR

            if (Brains is BaseThinker { EditMode: false } && thinker is BaseThinker _t)
                Brains = _t.DeepClone();
            else
            {
                Brains = thinker;
            }
#else
            Brains = thinker.DeepClone();
#endif
        }

        protected virtual IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return new IDebugDrawer[]
                { new EntityDebugDrawer.PositionDrawer(), new EntityDebugDrawer.BoundingBoxDrawer() };
        }

        private void initialize_debug()
        {
            var _drawers = SetupDebugDrawers().ToList();
            foreach (var _entityComponent in Components)
                _drawers = _drawers.Union(_entityComponent.Value.SetupDebugDrawers()).ToList();


            ((IOwner<IDebugDrawable>)this).Pets.UnionWith(_drawers);
            foreach (var _ownable in ((IOwner<IDebugDrawable>)this).Pets)
            {
                _ownable.ChangeOwner(this);
                if (_ownable is IDebugDrawer _drawer) _drawer.Initilize(DebugSystem);
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
        public virtual Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>();
        }

        public virtual void Load(Dictionary<string, object> save)
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
        public virtual void React(in IGlobalStateChanged e)
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
            DeInitializeBrains(Brains);
            cleanup_brains();
            DebugSystem.RemovePet(this);
            SaveSystem.Bus.UnSubscribe(this);
            GlobalStateBus.UnSubscribe(this);
            GameEventsBus.UnSubscribe(this);
            World.RemoveEntity(this);
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

        List<IDebugDrawable> IPet<IDebugDrawable>.Owners => owners;

        public void BroadcastEvent<T>(T @event) where T : struct
        {
            Bus.Send(@event);
            GameEventsBus.Send(@event);
        }

        protected NonEmptyList<IViewModel> GetDefaultView()
        {
            var _render = new RendererView();
            _render.InitOwner(this);
            return new NonEmptyList<IViewModel>(_render);
        }

        public class EntityBus : EventBusImpl, IEventBus
        {
        }


        public GameplayTagContainer GameplayTag
        {
            get { return Tag.Value.GameplayTag; }
        }
    }
}