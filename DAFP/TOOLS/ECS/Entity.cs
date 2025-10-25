using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using PixelRouge.Inspector;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using UnityGetComponentCache;
using Zenject;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS
{
    public abstract class Entity : MonoBehaviour, IEntity, IDisposable, IEntityPet, IRandomizeable, ISavable,
        IListener<IGlobalStateChanged>, IListener<OnSaveMadeOrLoaded>, IPet<IDebugDrawable>
    {
        // Serialized Fields
        [ReadOnly] [SerializeField] private string id;

        [InspectorName("RandomizingFrom0To100")] [Range(0, 100)] [SerializeField]
        private float Variety = 10;

        [NeverReadOnly] [SerializeField] public string ConfigStateName;

        [ShowHelpBoxIf("ShowHelp", "Are you sure you wanna change that? That can mess with the logic..")]
        [NeverReadOnly]
        public string ConfigDomainSufix;

        [HideInInspector] [SerializeField] public bool IsReadOnly;

        // Dependencies
        [Inject] protected World World;
        [Inject] protected ISaveSystem SaveSystem;

        [Inject] public IDebugSys<IGlobalGizmos, IMessenger> DebugSystem { get; }
        [Inject(Id = "GlobalStateBus")] protected IEventBus GlobalStateBus;

        // Components & Memory
        public Dictionary<Type, IEntityComponent> Components { get; } = new Dictionary<Type, IEntityComponent>();
        protected BlackBoard Memory;

        // Pets & Ownership
        protected readonly ISet<IOwnable<IEntity>> Pets = new HashSet<IOwnable<IEntity>>();
        ISet<IOwnable<IEntity>> IOwner<IEntity>.Pets => Pets;
        ISet<IOwnable<IDebugDrawable>> IOwner<IDebugDrawable>.Pets { get; } = new HashSet<IOwnable<IDebugDrawable>>();
        public List<IEntity> Owners { get; } = new List<IEntity>();

        // Public Properties & Events
        public bool ShowHelp => ConfigDomainSufix == null || ConfigDomainSufix.Length > 0;
        public string ID => id;

        public virtual string Name
        {
            get => GetType().FullName;
            set { }
        }

        public virtual Bounds Bounds { private set; get; }
        Bounds _bounds { set; get; }
        public virtual NonEmptyList<IViewModel> View { private set; get; }
        public abstract NonEmptyList<IViewModel> SetupView();
        public bool HasInitialized { get; set; }
        public event IEntity.TickCallBack OnTick;

        // Abstract Members
        protected virtual Type ConfigHandler => null;
        public abstract ITicker<IEntity> EntityTicker { get; }
        protected abstract void InitializeInternal();
        protected abstract void TickInternal();

        // Unity Messages
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(ConfigStateName))
                IsReadOnly = ((IEntity)this).GetConfigService()
                    .SaveExist(getFullDomainName(this), ConfigStateName, Name);

            if (IsReadOnly && !string.IsNullOrEmpty(ConfigStateName))
                LoadFromConfig();
        }

        private void Awake() => Initialize();

        private void Start() => OnStart();

        protected virtual void OnDestroy()
        {
            SaveSystem.Bus.UnSubscribe(this);
            GlobalStateBus.UnSubscribe(this);
            World.RemoveEntity(this);
        }

        // Configuration Methods
        [Button("Bake to Config.json", EButtonMode.EditorOnly)]
        public void SaveToConfig()
        {
            GatherComponents();
            var ent = (IEntity)this;
            string domain = ent.GetConfigSerializer().GetDomainName() + ConfigDomainSufix;

            ent.GetConfigService().Save(
                ent.GetConfigSerializer().Save(this),
                domain,
                ConfigStateName,
                ent.Name
            );

            IsReadOnly = ent.GetConfigService()
                .SaveExist(domain, ConfigStateName, Name);
        }

        [Button("Load baked from Config.json", EButtonMode.EditorOnly)]
        public void LoadFromConfig()
        {
            if (Components.Count == 0)
                GatherComponents();

            var ent = (IEntity)this;
            string domain = getFullDomainName(this);

            if (!ent.GetConfigService().SaveExist(domain, ConfigStateName, Name))
            {
                Debug.LogWarning($"{Name} cannot load the state of {ConfigStateName} (save does not exist)");
                return;
            }

            ent.GetConfigSerializer().Load(
                ent.GetConfigService().Load(domain, ConfigStateName, Name),
                ent
            );

            IsReadOnly = ent.GetConfigService()
                .SaveExist(domain, ConfigStateName, Name);
        }

        [Button("-- Delete baked from Config.json --", EButtonMode.EditorOnly)]
        public void DeleteConfig()
        {
            var ent = (IEntity)this;
            ent.GetConfigService().DeleteSave(
                getFullDomainName(this),
                ConfigStateName,
                Name
            );
            IsReadOnly = ent.GetConfigService()
                .SaveExist(getFullDomainName(this), ConfigStateName, Name);
        }

        [Button("Generate new ID", EButtonMode.EditorOnly)]
        public void GenNewID() => id = Guid.NewGuid().ToString();

        private string getFullDomainName(IEntity ent) =>
            ent.GetConfigSerializer().GetDomainName() + ConfigDomainSufix;

        // Initialization & Lifecycle
        public void Reset() => GenNewID();

        public void Initialize()
        {
            Memory = new BlackBoard(null, this);
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();

            World.RegisterEntity(this, EntityTicker);
            GatherComponents();

            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);

            SaveSystem.Bus.Subscribe(this);
            GlobalStateBus.Subscribe(this);

            foreach (var comp in Components.Values)
                comp.Initialize();

            View = SetupView();
            Bounds = CalculateBounds();
            InitializeInternal();
            InitializeDebug();
            HasInitialized = true;
        }

        private void InitializeDebug()
        {
            
            List<IDebugDrawer> drawers = SetupDebugDrawers().ToList();
            foreach (var _entityComponent in Components)
            {
                drawers = drawers.Union(_entityComponent.Value.SetupDebugDrawers()).ToList();
            }

            
            ((IOwner<IDebugDrawable>)this).Pets.UnionWith(drawers);
            foreach (var _ownable in ((IOwner<IDebugDrawable>)this).Pets)
            {
                _ownable.ChangeOwner(this);
                if (_ownable is IDebugDrawer drawer)
                {
                    drawer.Initilize(DebugSystem);
                }
            }

            DebugSystem.AddPet(this);
        }

        protected virtual IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return new IDebugDrawer[]
                { new EntityDebugDrawer.PositionDrawer(), new EntityDebugDrawer.BoundingBoxDrawer() };
        }

        public void OnStart()
        {
            if (Variety != 0)
                Randomize(Variety / 100f);

            foreach (var comp in Components.Values)
                comp.OnStartInternal();
        }

        public void Tick()
        {
            TickComponents();

            TickInternal();

            TickBounds();
            OnTick?.Invoke(this);
        }

        private void TickBounds()
        {
            //Recalculate bounds every couple of frames
            if (bounds_calc >= BoundsRefreshRate)
            {
                _bounds = CalculateBounds();
                bounds_calc = 0;
            }
            else
                bounds_calc++;

            var bvb = _bounds;
            bvb.center += transform.position;
            Bounds = bvb;
        }

        private void TickComponents()
        {
            foreach (var comp in Components.Values)
            {
                if (comp is IViewModel)
                    continue;
                if (comp.EntityComponentTicker == EntityTicker)
                    comp.Tick();
            }
        }

        private int bounds_calc = 0;
        private List<IDebugDrawable> owners;
        protected virtual int BoundsRefreshRate => 30;
        protected virtual Bounds CalculateBounds() => Utills.CalculateCombinedBounds(this);

        // Component Management
        private void GatherComponents()
        {
            Components.Clear();
            foreach (var comp in gameObject.GetComponents<IEntityComponent>())
                AddEntComponent(comp);
        }

        public void AddEntComponent(IEntityComponent component)
        {
            if (component is IStatBase stat && stat.SyncToBlackBoard && Memory != null)
            {
                Memory.Set(stat.Name, stat.GetAbsoluteValue());
                stat.OnUpdateValue += OnUpdateStat;
            }

            Components[component.GetType()] = component;
            component.Register(this);
        }

        private void OnUpdateStat(IStatBase stat)
        {
            if (stat.SyncToBlackBoard)
                Memory.Set(stat.Name, stat.GetAbsoluteValue());
        }


        public GameObject GetWorldRepresentation() => gameObject;
        public World GetWorld() => World;

        // Saving & Loading
        public virtual Dictionary<string, object> Save() => new Dictionary<string, object>();

        public virtual void Load(Dictionary<string, object> save)
        {
        }

        // Randomization
        public void Randomize(float margin01)
        {
            foreach (var comp in Components.Values)
                if (comp is IRandomizeable rnd)
                    rnd.Randomize(margin01);
        }

        // Event Reactions
        public virtual void React(in IGlobalStateChanged e)
        {
            if (e.Author.GetType() == ConfigHandler)
            {
                ConfigStateName = e.NewState.StateName;
                LoadFromConfig();
            }
        }

        public virtual void React(in OnSaveMadeOrLoaded e)
        {
        }

        public static implicit operator GameObject(Entity ent) => ent.GetWorldRepresentation();

        public void Dispose()
        {
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            //DO NOT OVERRIDE PLEASSSEEE
        }
        // protected virtual void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.green;
        //     if (Bounds.size != Vector3.zero)
        //         Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        //     else
        //     {
        //         Utills.DrawDebugPosition(Bounds.center, 0.3f, Color.green);
        //     }
        // }


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
    }
}