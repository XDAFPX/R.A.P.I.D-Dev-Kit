using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DAFP.GAME.Essential;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BuiltIn;
using JetBrains.Annotations;
using UnityEngine;

// TODO make entity sound events 
namespace DAFP.TOOLS.ECS
{
    public abstract class World : Manager<World>, IEntity
    {
        protected static readonly HashSet<IEntity> ENTITIES = new HashSet<IEntity>();
        protected static readonly HashSet<ITickerBase> TICKERS = new();

        protected static readonly HashSet<IGamePlayer> PLAYERS = new();
        public static ITicker<IEntityComponent> ComponentFixedUpdateTicker = new Ticker<IEntityComponent>(52);
        public static ITicker<IEntityComponent> ComponentUpdateTicker = new UpdateTicker<IEntityComponent>();
        public static ITicker<IEntity> UpdateTicker = new UpdateTicker<IEntity>();
        public static ITicker<IEntity> FixedUpdateTicker = new Ticker<IEntity>(52);

        public static void RegisterTicker([NotNull] ITickerBase ticker)
        {
            TICKERS.Add(ticker);
        }

        public static void SubscribeToOnTickEntities<T>(IEntity.TickCallBack callBack) where T : IEntity
        {
            foreach (var _entity in ENTITIES)
            {
                if (_entity is T breed)
                {
                    breed.OnTick += callBack;
                }
            } 
        }
        public static void RegisterEntity([NotNull] IEntity ent, [NotNull] ITicker<IEntity> ticker)
        {
            if (ent is IGamePlayer pl)
                PLAYERS.Add(pl);
            if (ENTITIES.Contains(ent))
                return;
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);
            ENTITIES.Add(ent);
            if (ent is IOwnable _ownable)
                Singleton.AddPet(_ownable);

            Debug.Log(
                $"Registered Entity... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
        }

        public static void RegisterCustomComponentTicker([NotNull] IEntityComponent ent,
            [NotNull] ITicker<IEntityComponent> ticker)
        {
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);

            Debug.Log(
                $"Registered CustomComponentTicker... ComponentName: {ent.GetType().Name}  ,WorldEntityName: {(ent.GetWorldRepresentation() ? ent.GetWorldRepresentation().name : "NoName")} ");
        }

        private IEnumerator Cycle(Action func, float delay)
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(delay);
                func.Invoke();
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public void Initialize()
        {
            id = Guid.NewGuid().ToString();
            GatherComponents();
            foreach (IEntityComponent _component in Components.Values)
            {
                _component.Initialize();
            }

            HasInitialized = true;
        }

        private void FixedUpdate()
        {
            foreach (var _ticker in TICKERS)
            {
                if (Mathf.Approximately(_ticker.UpdatesPerSecond, 52))
                {
                    _ticker.Tick();
                }
            }
        }

        private void Update()
        {
            foreach (var _ticker in TICKERS.OfType<UpdateTicker<IEntity>>())
            {
                _ticker.Tick();
            }

            Tick();
        }

        protected void Start()
        {
            foreach (var Tticker in TICKERS)
            {
                Tticker.OnStart();
                if (!Mathf.Approximately(Tticker.UpdatesPerSecond, 52) && Tticker is not UpdateTicker<IEntity>)
                    StartCoroutine(Cycle(Tticker.Tick, Tticker.DeltaTime));
            }


            OnStart();
        }

        public void OnStart()
        {
            foreach (IEntityComponent _entityComponent in Components.Values)
            {
                _entityComponent.OnStartInternal();
            }
        }

        public void Tick()
        {
            foreach (var component in Components.Values)
            {
                component.Tick();
            }
            OnTick?.Invoke(this);
        }

        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }

        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker => UpdateTicker;

        public Dictionary<Type, IEntityComponent> Components { get; }
            = new Dictionary<Type, IEntityComponent>();

        private void GatherComponents()
        {
            foreach (var component in gameObject.GetComponents<IEntityComponent>())
            {
                AddEntComponent(component);
            }
        }

        // 3. Lookup by type in the dictionary (Step 1 & 3)
        public T GetEntComponent<T>() where T : EntityComponent
        {
            if (Components.TryGetValue(typeof(T), out var comp))
                return comp as T;
            return null;
        }

        // 2. Add or replace the component under its actual type key (Step 1 & 2)
        public void AddEntComponent(IEntityComponent component)
        {
            Components[component.GetType()] = component;
            component.Register(this);
        }

        protected HashSet<IOwnable> Pets = new HashSet<IOwnable>();
        private string id;


        public virtual void AddPet(IOwnable pet)
        {
            Pets.Add(pet);
        }

        public string ID => id;
        public event IEntity.TickCallBack OnTick;

        public virtual void RemovePet(IOwnable pet)
        {
            Pets.Remove(pet);
        }

        public List<IEntity> Owners { get; } = new List<IEntity>();

        public IEntity GetCurrentOwner()
        {
            return null;
        }

        public IEntity GetExOwner()
        {
            return null;
        }

        public void ChangeOwner(IEntity newOwner)
        {
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