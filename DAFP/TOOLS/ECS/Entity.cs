using System;
using System.Collections.Generic;
using DAFP.TOOLS.BTs;
using UnityEngine;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Services;
using UnityGetComponentCache;
using Zenject;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS
{
    public abstract class Entity : MonoBehaviour, IEntity, IDisposable, IPet, IRandomizeable
    {
        [InspectorName("RandomizingFrom0To100")] [Range(0, 100)] [SerializeField]
        private float Variety = 10;

        // 1. Use a Dictionary for O(1) lookups instead of a HashSet
        public Dictionary<Type, IEntityComponent> Components { get; }
            = new Dictionary<Type, IEntityComponent>();

        protected BlackBoard Memory;
        [Inject]protected World world;


        private void GatherComponents()
        {
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
                Memory.Set(stat.Name, stat.GetAbsoluteValue());
                stat.OnUpdateValue += OnUpdateStat;
            }

            Components[component.GetType()] = component;
            component.Register(this);
        }

        public abstract ITicker<IEntity> EntityTicker { get; }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            
            Memory = new BlackBoard(null, this);
            id = Guid.NewGuid().ToString();
            world.RegisterEntity(this, EntityTicker);
            GatherComponents();
            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);
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
        private string id;


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
    }
}