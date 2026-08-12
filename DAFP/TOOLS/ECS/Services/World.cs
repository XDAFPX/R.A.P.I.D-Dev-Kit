using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BandoWare.GameplayTags;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Modifiers.Pegs;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.ViewModel;
using DAFP.TOOLS.Injection;
using JetBrains.Annotations;
using MessagePipe;
using NRandom;
using NUnit.Framework;
using R3;
using RapidLib.DAFP.TOOLS.Common;
using RapidLib.DAFP.TOOLS.Common.Utill;
using UGizmo;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    //A thing to manage entities without any consideration of any scene nor state
    public abstract class World : IEntity, IService, IOwnerOf<Ticker>, IInitializable, ITickable, IFixedTickable,
        IResetable, IMessageHandler<OnEntityRegisterEvent>, IMessageHandler<OnEntityDeregisterEvent>,IDisposable
    {
        public static readonly Ticker EMPTY_TICKER = new(0, new HashSet<IGameState>());
        public readonly Ticker EmptyTicker = EMPTY_TICKER;

        // [Inject] private ISubscriber[] subscribers;

        [Inject] private Adam adam;


        [Inject] public IGameStateHandler GameState { get; set; }
        [Inject] public IRandom Rng { get; set; }
        [Inject] public ICursorStateHandler CursorState { get; set; }
        [Inject] public IAssetFactory AssetFactory { get; set; }
        [Inject] private MessagePipe.ISubscriber<OnEntityDeregisterEvent> deregisterEvent;
        [Inject] private MessagePipe.ISubscriber<OnEntityRegisterEvent> registeredEvent;
        [Inject] private MessagePipe.IPublisher<OnEntityInitializedEvent> initializedEvent;
        [Inject] private MessagePipe.IPublisher<OnEntityBecomePlayerEvent> becomePlayerEvent;
        [Inject] private MessagePipe.IPublisher<OnEntityStopBeingPlayerEvent> stopPlayerEvent;
        [Inject] private MessagePipe.IPublisher<OnWorldInitializeEvent> worldInitEvent;

        [Inject] public ThinkerManager ThinkerManager;

        [Inject] public ISpacePartitioningSystem SpacePartitioningSystem { get; set; }

        [Inject(Id = IVideoGame.DEFAULT_UPDATE)]
        public ITicker DefaultUpdate;

        [Inject(Id = IVideoGame.EFFECTS_UPDATE)]
        public ITicker EffectsUpdate;

        [Inject(Id = IVideoGame.VIEW_MODEL_UPDATE)]
        public ITicker ViewUpdate;

        [Inject(Id = IVideoGame.THINKERS_UPDATE)]
        public ITicker ThinkerUpdate;

        [Inject(Id = IVideoGame.PHYSICS_UPDATE)]
        public ITicker PhysicsUpdate;

        public IEnumerable<IEntity> Entities => registeredEntities;
        private List<IEntity> registeredEntities = new();
        internal List<IEntity> StandardEntities = new();

        public IEnumerable<IPlayer> Players => players;
        private readonly HashSet<IPlayer> players = new();
        protected readonly List<ITickerBase> Tickers = new();

        public string Name { get; set; }


        private IDisposable sub;

        //--So let me break it down for ya
        //-- First comes Awake, so all entities register; --OnWorldLoadIsCalled directly after
        //--Then FINALLY comes Start and calls init_world that initializes every entity. But those with more priority go first. -- and then after OnWorldInit 
        /// --  DO NOT LISTEN TO THE GUY ABOVE HE IS ON DRUGS AND frankly I don't trust him
        ///-- SO
        ///-- it hoes like this
        /// -- NUMERO UNO :: Here goes Zenject's Initialize()
        /// -- every system intializes and subscribes to some events
        /// -- NUMERO DOS :: goes the bootstrap service
        /// -- Initialize() Int.Max oder looking for unconfigured entities that were preplaced into the scene and does the thing
        /// -- NUMERO TRES :: The bootstrap unnaunces every entity's creation and they all promptly get registered and initialized
        /// -- the important part that I forgor is that every entity's intialization order should be acording to their scene appearnce so that parents get intitalized first and so on
        /// -- NUMERO QUADRO :: Update loop
        public void Initialize()
        {
            init_world();
        }


        private void prepare_world()
        {
            id = Guid.NewGuid().ToString();
            Enabled = true;
            Memory = new BlackBoard(this);
            clean_up_entities();
            Name = GetType().Name;
            var d1 = registeredEvent.Subscribe(this);
            var d2 = deregisterEvent.Subscribe(this);
            sub = new CompositeDisposable(d1, d2);

            // Entities.Clear();
            // foreach (var _tickerBase in Tickers) _tickerBase.ResetToDefault();
            //
            // Tickers.Clear();
            //
            // DebugSystem.Log(this, $"the World ({this.Name}) is loading... ------- ");
        }


        private void init_world()
        {
            if (shutDowned || HasInitialized)
                return;
            prepare_world();
            HasInitialized = true;

            worldInitEvent.Publish(new(this));

            // Debug.Log($"[World] ({this.Name}) initialized and loaded. (SCENE: ) ");
            // DebugSystem.Log(this, $"the World ({this.Name}) initialized and loaded. ");
        }

        internal void HandleEntityInitialization(IEntity ent)
        {
            if (ent.HasInitialized)
                return;
            if (ent is IEntityLogic _logic)
            {
                _logic.Initialize();
                initializedEvent.Publish(new OnEntityInitializedEvent(ent));
            }
        }

        void IMessageHandler<OnEntityRegisterEvent>.Handle(OnEntityRegisterEvent message)
        {
            register_entity(message.Entity, message.Entity.EntityTicker);
        }

        void IMessageHandler<OnEntityDeregisterEvent>.Handle(OnEntityDeregisterEvent message)
        {
            un_register_entity(message.Entity);
        }

        private bool shutDowned;

        public void Shutdown()
        {
            if (shutDowned)
                return;
            foreach (var _entity in Entities)
            {
                adam.Destroy(_entity).Forget();
            }

            ResetToDefault();

            shutDowned = true;
            Debug.Log($"[World] ({this.Name}) has shutdown ");
        }

        //-- Other stuff -----------------------------------------------------------------


        private void register_entity(IEntity ent, ITicker ticker)
        {
            if (ReferenceEquals(ent, this))
                return;
            if (ent == null || ticker == null)
                return;
            if (Entities.Contains(ent))
                return;
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);
            add_ent(ent);
            if (ent.GetWorldRepresentation().TryGetComponent(out IPlayer _player))
                register_player(_player);
            // if (HasInitialized) //--TODO fix
            //     ent.Initialize();

            // Debug.Log(
            //     $"[World]: Registered Entity... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
        }


        private void un_register_player(IPlayer player)
        {
            if (!players.Contains(player))
                return;
            var _ev = new OnEntityStopBeingPlayerEvent(player.Body, player.Data);
            players.Remove(player);
            stopPlayerEvent.Publish(_ev);
        }

        private void register_player(IPlayer player)
        {
            if (players.Contains(player))
                return;
            var _ev = new OnEntityBecomePlayerEvent(player.Body, player.Data);
            players.Add(player);
            becomePlayerEvent.Publish(_ev);
        }

        public bool IsRegistered(IEntity ent) => registeredEntities.Contains(ent);

        private void un_register_entity([NotNull] IEntity ent)
        {
            try
            {
                un_register_player(ent.TryGetPlayer());
                remove_ent(ent);
                Tickers.Find((@base => ent.EntityTicker == @base))?.Remove(ent);
                foreach (var _entityComponent in ent.GetWorldRepresentation().GetComponents<IEntityComponent>())
                    if (_entityComponent.EntityComponentTicker != ent.EntityTicker)
                        _entityComponent.EntityComponentTicker.Remove(_entityComponent);
            }
            catch (Exception _e)
            {
                Debug.LogWarning($"[World] :: Unregistered entity : {ent.Name} was removed :: {_e} ");
            }
        }


        public void RegisterCustomComponentTicker([NotNull] IEntityComponent ent,
            [NotNull] ITicker ticker)
        {
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);

            Debug.Log(
                $"Registered CustomComponentTicker... ComponentName: {ent.GetType().Name}  ,WorldEntityName: {(ent.GetWorldRepresentation() ? ent.GetWorldRepresentation().name : "NoName")} ");
        }


        private void safe_tick(ITickerBase ticker)
        {
            if (ticker.IsAllowedToTick(GameState.Current))
                ticker.Tick();
        }

        public void FixedTick()
        {
            foreach (var _ticker in Tickers.OfType<FixedUpdateTicker>()) safe_tick(_ticker);
        }

        public void Tick()
        {
            foreach (var _ticker in Tickers.OfType<UpdateTicker>()) safe_tick(_ticker);

            foreach (var _ticker in Tickers.OfType<UpdateTicker>()) safe_tick(_ticker);

            foreach (var _tickerBase in Tickers.OfType<Ticker>())
            {
                if (_tickerBase.UpdatesPerSecond == 0)
                    continue;
                _tickerBase.Elapsed += Time.deltaTime;
                if (_tickerBase.Elapsed >= _tickerBase.DeltaTime)
                {
                    _tickerBase.Elapsed = 0;
                    safe_tick(_tickerBase);
                }
            }
        }

        public IPlayer MakePlayer(PlayerData data)
        {
            var _ent = data.Memory.GetSelf();
            if (_ent == null)
                return null;
            if (_ent.GetWorldRepresentation().GetComponent<IPlayer>() != null)
                return null;
            var _player = _ent.AddAndRegisterComponent<Player>();
            
            _player.Data = data;
            register_player(_player);
            return _player;
        }

        public void MovePlayer([NotNull] Player player, [NotNull] IEntity newOwner)
        {
            if (player.Body == newOwner)
                throw new ArgumentException("tried to move the player on it self");
            if (newOwner.GetWorldRepresentation().GetComponent<IPlayer>() != null)
                throw new ArgumentException("tried to move the player to a another player object");


            var _oldData = player.Data;
            un_register_player(player);
            GameObject.Destroy(player);
            var _newPlayer = newOwner.AddAndRegisterComponent<Player>();
            _newPlayer.Data = _oldData.SetData(new BlackBoard(newOwner, _oldData.Memory.GetFullData()));
            register_player(_newPlayer);
        }

        public void RegisterTicker([NotNull] ITickerBase ticker)
        {
            if (Tickers.Contains(ticker))
                return;

            Tickers.Add(ticker);

            // it sorts highest-to-lowest.
            Tickers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        // public void SubscribeToOnTickEntities<T>(IEntity.TickCallBack callBack) where T : IEntity
        // {
        //     foreach (var _entity in RegisteredEntities)
        //         if (_entity is T _breed)
        //             _breed.OnTick += callBack;
        // }

        //--Ent Stuff -----------------------------------------------------------------------------------------------------------------------------------
        public IStatContainer Stats => new DummyStatContainer();


        public ICollection<IViewModel> View { get; } = new EmptyView().ToEnumerable().Cast<IViewModel>().ToList();
        public BlackBoard Memory { get; private set; }

        public bool HasInitialized { get; set; }

        public IThinker Brains
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ITicker EntityTicker => EmptyTicker;
        public string ID => id;

        public World GetWorld()
        {
            return this;
        }

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(23123132131321, 31232323231, 3132331321111));
        public Bounds CachedBounds => Bounds;
        public IVector EyeVector => new V3();


        private string id;
        private IEnumerable<IDebugDrawable> pets = new List<IDebugDrawable>();
        private IEnumerable<IViewModel> pets1 = new List<IViewModel>();
        private IEnumerable<IStatBase> pets2 = new List<IStatBase>();
        private IEnumerable<IStatModifierBase> pets3 = new List<IStatModifierBase>();
        private IEnumerable<PegModifier> pets4 = new List<PegModifier>();
        private IEnumerable<IEntityAccessory> pets5 = new List<IEntityAccessory>();


        public GameObject GetWorldRepresentation()
        {
            return new GameObject();
        }

        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => pets;

        public void AddPet(Ticker pet)
        {
            RegisterTicker(pet);
        }

        public bool RemovePet(Ticker pet)
        {
            return false;
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
        IEnumerable<Ticker> IOwnerOf<Ticker>.Pets => Tickers.OfType<Ticker>();
        public IEnumerable<object> AbsolutePets => Tickers.Cast<object>().Union(Entities);
        public List<IEntity> Children => registeredEntities;

        public ISaveData Save()
        {
            return new GenericSaveData(new()); // TODO think if IEntity should be ISavable
        }

        public void Load(ISaveData saveData)
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


        public void Transition(IWorldTransition transition)
        {
            if (shutDowned)
                return;
            if (transition is not IWorldTransitionInternal _internal)
                return;
            _internal.Transition(this).Forget();
        }

        public GameplayTagContainer GameplayTag
        {
            get => GameplayTagContainer.Empty;
            set { }
        }

        public void ResetToDefault()
        {
            Tickers.ForEach((@base => @base.ResetToDefault()));
            clear_entities();
            players.Clear();
            HasInitialized = false;
            shutDowned = false;
            Debug.Log($"[World]: The World was reset");
        }

        private void add_ent(IEntity ent)
        {
            registeredEntities.Add(ent);
            try { SpacePartitioningSystem?.Add(ent); } catch { }
            invalidate_entity_caches();
        }

        private void remove_ent(IEntity ent)
        {
            registeredEntities.Remove(ent);
            try { SpacePartitioningSystem?.Remove(ent); } catch { }
            invalidate_entity_caches();
        }

        private void clean_up_entities()
        {
            registeredEntities = Entities.ClearOfNulls().ToList();
            try { SpacePartitioningSystem?.Clear(); foreach (var e in registeredEntities) SpacePartitioningSystem?.Add(e); } catch { }
            invalidate_entity_caches();
        }

        private void clear_entities()
        {
            registeredEntities.Clear();
            try { SpacePartitioningSystem?.Clear(); } catch { }
            invalidate_entity_caches();
        }

        private void invalidate_entity_caches()
        {
            StandardEntities = Entities.Where((entity => entity is not ITechnicalEntity)).ToList();
            InvalidateEntityCaches();
        }

        protected virtual void InvalidateEntityCaches()
        {
        }

        // public T Create<T>() where T : Component, IEntity --moved to adam
        // {
        //     return Create<T>(Vector3.zero.ToGeneric());
        // }
        //
        // public T Create<T>(IVector pos) where T : Component, IEntity
        // {
        //     return Create<T>(typeof(T).Name, pos);
        // }
        //
        // public T Create<T>(string name, IVector pos) where T : Component, IEntity
        // {
        //     return Create<T>(name, (entity => entity.Pos(pos)));
        // }
        //
        // public T Create<T>(string name, Action<T> process) where T : Component, IEntity
        // {
        //     var _obj = new GameObject(name);
        //     var _ent = _obj.AddComponent<T>();
        //     process.Invoke(_ent);
        //     AssetFactory.Create(_obj);
        //     return _ent;
        // }

        // public async UniTask<T> Clone<T>(T ent) where T : IEntity //--TODO make this better
        // {
        //     var clone = await GameObject.InstantiateAsync(ent.GetWorldRepresentation());
        //     AssetFactory.Create(clone.FirstOrDefault());
        //     return clone.FirstOrDefault()!.GetComponent<T>();
        // }


        // public IEntity Create()
        // {
        //     return Create<EmptyEntity>();
        // }
        //
        // public IEntity Create(IVector param)
        // {
        //     return Create<EmptyEntity>(param);
        // }
        //
        // public IEntity Create(string param1, IVector param2)
        // {
        //     return Create<EmptyEntity>(param1, param2);
        // }
        public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem { get; } = null;
        public void Dispose()
        {
            sub.Dispose();
        }
    }
}