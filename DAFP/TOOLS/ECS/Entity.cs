using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.BTs;
using UnityEngine;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using PixelRouge.Inspector;
using UnityEventBus;
using UnityGetComponentCache;
using Zenject;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS
{
    public abstract class Entity : MonoBehaviour, IEntity, IDisposable, IPet, IRandomizeable, ISavable,
        IListener<IGlobalStateChanged>, IListener<OnSaveMadeOrLoaded>
    {
        [ReadOnly] [SerializeField] private string id;

        [InspectorName("RandomizingFrom0To100")] [Range(0, 100)] [SerializeField]
        private float Variety = 10;

        [NeverReadOnly] [SerializeField] public string ConfigStateName;

        [ShowHelpBoxIf("ShowHelp", "Are you sure you wanna change that? That can mess with the logic..")]
        [NeverReadOnly]
        public string ConfigDomainSufix;

        public Dictionary<Type, IEntityComponent> Components { get; }
            = new Dictionary<Type, IEntityComponent>();

        protected BlackBoard Memory;
        [Inject] protected World world;
        [Inject] protected ISaveSystem saveSystem;
        [Inject(Id = "GlobalStateBus")] protected IEventBus GlobalStateBus;

        public bool ShowHelp => ConfigDomainSufix == null || ConfigDomainSufix.Length > 0;

        [Button("Bake to Config.json", EButtonMode.EditorOnly)]
        public void SaveToConfig()
        {
            GatherComponents();
            var ent = ((IEntity)this);
            ent.GetConfigService().Save(ent.GetConfigSerializer().Save(this),
                ent.GetConfigSerializer().GetDomainName() + ConfigDomainSufix,
                ConfigStateName, ent.Name);
            IsReadOnly =
                ((IEntity)this).GetConfigService().SaveExist(getFullDomainName(this), ConfigStateName, Name);
        }

        [Button("Load baked from Config.json", EButtonMode.EditorOnly)]
        public void LoadFromConfig()
        {
            if (Components == null || Components.Count == 0)
                GatherComponents();
            var ent = ((IEntity)this);

            if (!ent.GetConfigService().SaveExist(ent.GetConfigSerializer().GetDomainName() + ConfigDomainSufix,
                    ConfigStateName, ent.Name))
            {
                Debug.LogWarning($"{Name} cannot load the state of {ConfigStateName} (save does not exist )");
                return;
            }

            ent.GetConfigSerializer().Load(
                ent.GetConfigService().Load(ent.GetConfigSerializer().GetDomainName() + ConfigDomainSufix,
                    ConfigStateName, ent.Name), ent);
            IsReadOnly =
                ((IEntity)this).GetConfigService().SaveExist(getFullDomainName(this), ConfigStateName, Name);
        }

        [Button("-- Delete baked from Config.json --", EButtonMode.EditorOnly)]
        public void DeleteConfig()
        {
            var ent = ((IEntity)this);
            ent.GetConfigService().DeleteSave(getFullDomainName(this), ConfigStateName, Name);
            IsReadOnly =
                ((IEntity)this).GetConfigService().SaveExist(getFullDomainName(this), ConfigStateName, Name);
        }

        [Button("Generate new ID", EButtonMode.EditorOnly)]
        public void GenNewID()
        {
            id = Guid.NewGuid().ToString();
        }

        protected string getFullDomainName(IEntity ent) =>
            ent.GetConfigSerializer().GetDomainName() + ConfigDomainSufix;

        [HideInInspector] [SerializeField] public bool IsReadOnly;

        private void OnValidate()
        {
            if (!String.IsNullOrEmpty(ConfigStateName))
                IsReadOnly =
                    ((IEntity)this).GetConfigService().SaveExist(getFullDomainName(this), ConfigStateName, Name);
            if (IsReadOnly && !String.IsNullOrEmpty(ConfigStateName))
                LoadFromConfig();
        }

        // 1. Use a Dictionary for O(1) lookups instead of a HashSet


        private void GatherComponents()
        {
            Components.Clear();
            foreach (var component in gameObject.GetComponents<IEntityComponent>())
            {
                AddEntComponent(component);
            }
        }


        private void OnUpdateStat(IStatBase stat)
        {
            if (!stat.SyncToBlackBoard)
                return;
            Memory.Set(stat.Name, stat.GetAbsoluteValue());
        }

        // 2. Add or replace the component under its actual type key (Step 1 & 2)
        public void AddEntComponent(IEntityComponent component)
        {
            if (component is IStatBase { SyncToBlackBoard: true } stat)
            {
                if (Memory != null)
                {
                    Memory.Set(stat.Name, stat.GetAbsoluteValue());
                    stat.OnUpdateValue += OnUpdateStat;
                }
            }

            Components[component.GetType()] = component;
            component.Register(this);
        }

        protected virtual Type ConfigHandler => null;
        public abstract ITicker<IEntity> EntityTicker { get; }

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            OnStart();
        }

        public void Reset()
        {
            GenNewID();
        }

        public void Initialize()
        {
            Memory = new BlackBoard(null, this);
            if (String.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
            world.RegisterEntity(this, EntityTicker);
            GatherComponents();
            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);
            saveSystem.Bus.Subscribe(this);
            GlobalStateBus.Subscribe(this);
            foreach (IEntityComponent _component in Components.Values)
            {
                _component.Initialize();
            }

            InitializeInternal();
            HasInitialized = true;
        }

        public void OnStart()
        {
            if (Variety != 0)
                Randomize(Variety / 100);
            foreach (IEntityComponent _entityComponent in Components.Values)
            {
                _entityComponent.OnStartInternal();
            }
        }

        public void Tick()
        {
            foreach (var component in Components.Values)
            {
                if (component.EntityComponentTicker == EntityTicker)
                    component.Tick();
            }

            TickInternal();
            OnTick?.Invoke(this);
        }

        protected abstract void TickInternal();
        protected abstract void InitializeInternal();
        public bool HasInitialized { get; set; }

        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }

        protected HashSet<IOwnable> Pets = new HashSet<IOwnable>();


        public virtual void AddPet(IOwnable pet)
        {
            Pets.Add(pet);
        }

        public string ID => id;
        public event IEntity.TickCallBack OnTick;

        public World GetWorld()
        {
            return world;
        }

        public virtual void RemovePet(IOwnable pet)
        {
            Pets.Remove(pet);
        }

        public List<IEntity> Owners { get; } = new List<IEntity>();

        public IEntity GetCurrentOwner()
        {
            return Owners.Count > 0 ? Owners[^1] : null;
        }

        public IEntity GetExOwner()
        {
            return Owners.Count >= 2 ? Owners[^2] : null;
        }

        public void ChangeOwner(IEntity newOwner)
        {
            var current = GetCurrentOwner();
            if (current == null)
            {
                if (newOwner == null) return;
                newOwner.AddPet(this);
                Owners.Add(newOwner);
            }
            else
            {
                current.RemovePet(this);
                Owners.Add(newOwner);
                newOwner?.AddPet(this);
            }
        }

        protected virtual void OnDestroy()
        {
            saveSystem.Bus.UnSubscribe(this);
            GlobalStateBus.UnSubscribe(this);
            world.RemoveEntity(this);
        }

        public void Randomize(float margin01)
        {
            foreach (var component in Components.Values)
            {
                if (component is IRandomizeable randomizeable)
                    randomizeable.Randomize(margin01);
            }
        }

        public virtual Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>();
        }

        public virtual void Load(Dictionary<string, object> save)
        {
        }

        public virtual string Name
        {
            get => GetType().FullName;
            set { }
        }

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
    }
}