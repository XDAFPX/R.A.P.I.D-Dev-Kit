using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using BandoWare.GameplayTags;
using DAFP.GAME.Assets;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Modifiers.Pegs;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.ViewModel;
using JetBrains.Annotations;
using RapidLib.DAFP.TOOLS.Common;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    public abstract class World : MonoBehaviour, IEntity, IService, IOwnerOf<Ticker<IEntity>>

    {
        public static readonly Ticker<IEntity> EMPTY_TICKER = new(0, new HashSet<IGlobalGameState>());
        public readonly Ticker<IEntity> EmptyTicker = EMPTY_TICKER;

        [Inject(Id = "DefaultUpdateEntityGameplayTicker")]
        public ITicker<IEntity> DefaultGameplayTicker { get; }

        public readonly List<IEntity> Entities = new();
        protected readonly List<ITickerBase> Tickers = new();

        public string Name
        {
            get => name;
            set => name = value;
        }

        //--So let me break it down for ya
        //-- First comes Awake so all entities register;
        //--Then FINALLY comes Start and calls init_world that initializes every entity. But those with more priority go first.


        public void Start()
        {
            init_world();
        }

        private void prepare_world()
        {
            id = Guid.NewGuid().ToString();
            Enabled = true;
            Memory = new BlackBoard(null, this);


            // Entities.Clear();
            // foreach (var _tickerBase in Tickers) _tickerBase.ResetToDefault();
            //
            // Tickers.Clear();
            //
            // DebugSystem.Log(this, $"the World ({this.Name}) is loading... ------- ");
        }

        private void init_world()
        {
            prepare_world();
            var _prioritized = Entities.OfType<IPrioritized>();
            var _nonPrioritized = Entities.Except(_prioritized.Cast<IEntity>()).Cast<IPetOwnerTreeOf<IEntity>>();

            _prioritized.PriorityForeach((prioritized1 => ((IEntity)prioritized1).Initialize()));

            foreach (var _entity in _nonPrioritized)
            {
                if (_entity.GetCurrentOwner() != null) continue;
                 ((IEntity)_entity).Initialize();
            }

            HasInitialized = true;
            
            Debug.Log($"The World ({this.Name}) initialized and loaded. ");
            // DebugSystem.Log(this, $"the World ({this.Name}) initialized and loaded. ");
        }

        public void Shutdown()
        {
            //--Do stuff
        }


        //-- Stuff

        private void FixedUpdate()
        {
            FixedTick();
        }


        private void Update()
        {
            Tick();
        }


        //-- Other stuff -----------------------------------------------------------------


        public void RegisterEntity(IEntity ent, ITicker<IEntity> ticker)
        {
            if (ReferenceEquals(ent, this))
                return;
            if (ent == null || ticker == null)
                return;
            if (Entities.Contains(ent))
                return;
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);
            Entities.Add(ent);
            if (HasInitialized)
                ent.Initialize();

            Debug.Log(
                $"Registered Entity... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
        }

        public bool IsRegistered(IEntity ent) => Entities.Contains(ent);

        public void RemoveEntity([NotNull] IEntity ent)
        {
            try
            {
                Entities.Remove(ent);
                ent.EntityTicker.Subscribed.Remove(ent);
                foreach (var _entityComponent in ent.Components)
                    if (_entityComponent.Value.EntityComponentTicker != ent.EntityTicker)
                        _entityComponent.Value.EntityComponentTicker.Remove(_entityComponent.Value);
            }
            catch (Exception _e)
            {
                Debug.LogWarning($"Unregistered entity : {ent.Name} was removed :: {_e} ");
            }
        }

        public void RegisterCustomComponentTicker([NotNull] IEntityComponent ent,
            [NotNull] ITicker<IEntityComponent> ticker)
        {
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);

            Debug.Log(
                $"Registered CustomComponentTicker... ComponentName: {ent.GetType().Name}  ,WorldEntityName: {(ent.GetWorldRepresentation() ? ent.GetWorldRepresentation().name : "NoName")} ");
        }


        public void FixedTick()
        {
            foreach (var _ticker in Tickers.OfType<FixedUpdateTicker<IEntity>>()) SafeTick(_ticker);

            foreach (var _ticker in Tickers.OfType<FixedUpdateTicker<IEntityComponent>>()) SafeTick(_ticker);
        }

        public void SafeTick(ITickerBase ticker)
        {
            if (ticker.IsAllowedToTick(GameState.Current()))
                ticker.Tick();
        }


        public void Tick()
        {
            foreach (var _ticker in Tickers.OfType<UpdateTicker<IEntity>>()) SafeTick(_ticker);

            foreach (var _ticker in Tickers.OfType<UpdateTicker<IEntityComponent>>()) SafeTick(_ticker);

            foreach (var _tickerBase in Tickers.OfType<Ticker<ITickable>>())
            {
                if (_tickerBase.UpdatesPerSecond == 0)
                    continue;
                _tickerBase.Elapsed += Time.deltaTime;
                if (_tickerBase.Elapsed >= _tickerBase.DeltaTime)
                {
                    _tickerBase.Elapsed = 0;
                    SafeTick(_tickerBase);
                }
            }
        }


        public IEntity SpawnEmptyEntity(Vector3 pos)
        {
            var _obj = new GameObject("EntEmpty");
            _obj.transform.position = pos;
            return _obj.AddEmptyEntity(AssetFactory);
        }

        public void RegisterTicker([NotNull] ITickerBase ticker)
        {
            if (Tickers.Contains(ticker))
                return;

            Tickers.Add(ticker);
            Tickers.Sort();
        }

        public void SubscribeToOnTickEntities<T>(IEntity.TickCallBack callBack) where T : IEntity
        {
            foreach (var _entity in Entities)
                if (_entity is T _breed)
                    _breed.OnTick += callBack;
        }


        //--Ent Stuff -----------------------------------------------------------------------------------------------------------------------------------


        public IThinker Brains => null;
        public IStatContainer Stats => new DummyStatContainer();

        public void Initialize() => init_world();

        public void DeInitializeBrains(IThinker thinker)
        {
        }

        public void InitializeBrains(IThinker thinker)
        {
        }

        public NonEmptyList<IViewModel> View { get; } = new NonEmptyList<IViewModel>(new EmptyView());
        public BlackBoard Memory { get; private set; }
        public Dictionary<Type, IEntityComponent> Components { get; }

        public void AddEntComponent(IEntityComponent component)
        {
        }

        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker => EmptyTicker;
        public string ID => id;
        public event IEntity.TickCallBack OnTick;

        public World GetWorld()
        {
            return this;
        }

        public IEventBus Bus => GlobalStateBus;
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(23123132131321, 31232323231, 3132331321111));
        public Bounds CachedBounds => Bounds;
        public IVectorBase EyeVector => new V3();

        public void Remove(EntityRemovalReason removalReason)
        {
            Shutdown();
        }

        public void BroadcastEvent<T>(T @event) where T : struct
        {
            ((IEventBus)Bus).Send(@event);
        }


        private string id;
        private IEnumerable<IDebugDrawable> pets = new List<IDebugDrawable>();
        private IEnumerable<IViewModel> pets1 = new List<IViewModel>();
        private IEnumerable<IStatBase> pets2 = new List<IStatBase>();
        private IEnumerable<IStatModifierBase> pets3 = new List<IStatModifierBase>();
        private IEnumerable<PegModifier> pets4 = new List<PegModifier>();
        private IEnumerable<IEntityAccessory> pets5 = new List<IEntityAccessory>();


        [Inject(Id = "GlobalStateBus")] public IEventBus GlobalStateBus;
        [Inject] public IGlobalGameStateHandler GameState { get; set; }

        [Inject] public IGlobalCursorStateHandler CursorState { get; set; }
        [Inject] public IAssetFactory.DefaultAssetFactory AssetFactory { get; set; }
        public IDebugSys<IGlobalGizmos, IMessenger> DebugSystem { get; }


        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }


        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => pets;

        public void AddPet(Ticker<IEntity> pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(Ticker<IEntity> pet)
        {
            throw new NotImplementedException();
        }

        public void AddPet(IEntityAccessory pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(IEntityAccessory pet)
        {
            throw new NotImplementedException();
        }

        public void AddPet(PegModifier pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(PegModifier pet)
        {
            throw new NotImplementedException();
        }

        public void AddPet(IStatModifierBase pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(IStatModifierBase pet)
        {
            throw new NotImplementedException();
        }

        public void AddPet(IStatBase pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(IStatBase pet)
        {
            throw new NotImplementedException();
        }

        public void AddPet(IViewModel pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(IViewModel pet)
        {
            throw new NotImplementedException();
        }

        public void AddPet(IDebugDrawable pet)
        {
            throw new NotImplementedException();
        }

        public bool RemovePet(IDebugDrawable pet)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IViewModel> IOwnerOf<IViewModel>.Pets => pets1;

        IEnumerable<IStatBase> IOwnerOf<IStatBase>.Pets => pets2;

        IEnumerable<IStatModifierBase> IOwnerOf<IStatModifierBase>.Pets => pets3;

        IEnumerable<PegModifier> IOwnerOf<PegModifier>.Pets => pets4;

        IEnumerable<IEntityAccessory> IOwnerOf<IEntityAccessory>.Pets => pets5;

        IEnumerable<Ticker<IEntity>> IOwnerOf<Ticker<IEntity>>.Pets => Tickers.OfType<Ticker<IEntity>>();

        public IEnumerable<object> AbsolutePets => Tickers.Cast<object>().Union(Entities);
        public List<IEntity> Children => Entities;

        public Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>();
        }

        public void Load(Dictionary<string, object> save)
        {
        }


        public List<IEntity> Owners => new List<IEntity>();

        public IDebugDrawable GetCurrentOwner()
        {
            return null;
        }

        public void ChangeOwner(IDebugDrawable newOwner)
        {
        }

        public bool Enabled { get; private set; }

        public void Enable()
        {
            init_world();
        }

        public void Disable()
        {
            Shutdown();
        }


        public GameplayTagContainer GameplayTag => GameplayTagContainer.Empty;
    }
}