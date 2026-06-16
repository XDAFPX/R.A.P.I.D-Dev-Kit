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
using RapidLib.DAFP.TOOLS.Common;
using RapidLib.DAFP.TOOLS.Common.Utill;
using UGizmo;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    //A thing to manage entities without any consideration of any scene nor state
    public abstract class World : IEntity, IService, IOwnerOf<Ticker>, IInitializable, ITickable, IFixedTickable,
        IResetable,
        IFactory<IEntity>, IFactory<IVector, IEntity>, IFactory<string, IVector, IEntity>

    {
        public static readonly Ticker EMPTY_TICKER = new(0, new HashSet<IGameState>());
        public readonly Ticker EmptyTicker = EMPTY_TICKER;

        [Inject] private ISubscriber[] subscribers;

        [Inject(Id = IVideoGame.DEFAULT_UPDATE)]
        public ITicker DefaultUpdate;

        [Inject(Id = IVideoGame.EFFECTS_UPDATE)]
        public ITicker EffectsUpdate;

        [Inject(Id = IVideoGame.VIEW_MODEL_UPDATE)]
        public ITicker ViewUpdate;

        [Inject(Id = IVideoGame.PHYSICS_UPDATE)]
        public ITicker PhysicsUpdate;

        public List<IEntity> Entities = new();
        public IEnumerable<IPlayer> Players => players;
        private readonly HashSet<IPlayer> players = new();
        protected readonly List<ITickerBase> Tickers = new();

        public string Name { get; set; }

        private SingleTask loadWorldTask = new();

        //--So let me break it down for ya
        //-- First comes Awake, so all entities register; --OnWorldLoadIsCalled directly after
        //--Then FINALLY comes Start and calls init_world that initializes every entity. But those with more priority go first. -- and then after OnWorldInit
        private string lastLoadedScene;

        public void Initialize()
        {
            subscribers.ForEach(subscriber => Bus.Subscribe(subscriber));
            SceneManager.sceneLoaded -= OnSceneLoaded; //--For some insane reason it keeps piling up and stops only after the domain reload
            SceneManager.sceneLoaded += OnSceneLoaded;

            var _currentScene = SceneManager.GetActiveScene().name; //-- so I have to do this bullshit
            if (lastLoadedScene != _currentScene)
            {
                lastLoadedScene = _currentScene;
                loadWorldTask.Run(load_world);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            lastLoadedScene = scene.name;
            loadWorldTask.Run(load_world);
        }

        private async UniTask load_world(CancellationToken ct)
        {
            await UniTask.DelayFrame(1, cancellationToken: ct);
            this.BroadcastEvent(new OnWorldLoadEvent(this));
            await UniTask.DelayFrame(2, cancellationToken: ct); //--wait for everybody to catch up
            init_world();
        }

        private void prepare_world()
        {
            id = Guid.NewGuid().ToString();
            Enabled = true;
            Memory = new BlackBoard(this);
            Entities = Entities.ClearOfNulls().ToList();
            Name = GetType().Name;
            // Entities.Clear();
            // foreach (var _tickerBase in Tickers) _tickerBase.ResetToDefault();
            //
            // Tickers.Clear();
            //
            // DebugSystem.Log(this, $"the World ({this.Name}) is loading... ------- ");
        }

        private void init_world()
        {
            if (shutDowned)
                return;
            prepare_world();
            var _prioritized = Entities.OfType<IPrioritized>().ToArray();
            var _nonPrioritized = Entities.Except(_prioritized.Cast<IEntity>()).Cast<IPetOwnerTreeOf<IEntity>>()
                .ToArray();

            _prioritized.PriorityForeach((prioritized1 => ((IEntity)prioritized1).Initialize()));
            foreach (var _entity in _nonPrioritized)
            {
                if (_entity == null) continue;
                if (_entity.GetCurrentOwner() != null) continue;
                ((IEntity)_entity).Initialize();
            }

            HasInitialized = true;

            BroadcastEvent(new OnWorldInitEvent(this));

            Debug.Log($"[World] ({this.Name}) initialized and loaded. (SCENE: {lastLoadedScene}) ");
            // DebugSystem.Log(this, $"the World ({this.Name}) initialized and loaded. ");
        }

        private bool shutDowned;

        public void Shutdown()
        {
            foreach (var _entity in Entities)
            {
                _entity.Remove(EntityRemovalReason.DEFAULT);
            }

            ResetToDefault();
            shutDowned = true;
            Debug.Log($"[World] ({this.Name}) has shutdown ");
        }

        //-- Other stuff -----------------------------------------------------------------

        public void RegisterEntity(IEntity ent, ITicker ticker)
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
            if (ent.GetWorldRepresentation().TryGetComponent(out IPlayer _player))
                register_player(_player);
            if (HasInitialized) //--TODO fix
                ent.Initialize();

            Debug.Log(
                $"[World]: Registered Entity... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
        }

        private void un_register_player(IPlayer player)
        {
            if (!players.Contains(player))
                return;
            var _ev = new OnEntityStopBeingPlayer(player.Body, player.Data);
            player.Body.BroadcastEvent(_ev);
            players.Remove(player);
        }

        private void register_player(IPlayer player)
        {
            if (players.Contains(player))
                return;
            var _ev = new OnEntityBecomePlayerEvent(player.Body, player.Data);
            player.Body.BroadcastEvent(_ev);
            players.Add(player);
        }

        public bool IsRegistered(IEntity ent) => Entities.Contains(ent);

        public void RemoveEntity([NotNull] IEntity ent)
        {
            try
            {
                un_register_player(ent.TryGetPlayer());
                Entities.Remove(ent);
                ent.EntityTicker.Subscribed.Remove(ent);
                foreach (var _entityComponent in ent.Components)
                    if (_entityComponent.Value.EntityComponentTicker != ent.EntityTicker)
                        _entityComponent.Value.EntityComponentTicker.Remove(_entityComponent.Value);
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

        public void FixedTick()
        {
            foreach (var _ticker in Tickers.OfType<FixedUpdateTicker>()) SafeTick(_ticker);
        }

        public void SafeTick(ITickerBase ticker)
        {
            if (ticker.IsAllowedToTick(GameState.Current))
                ticker.Tick();
        }

        public void Tick()
        {
            foreach (var _ticker in Tickers.OfType<UpdateTicker>()) SafeTick(_ticker);

            foreach (var _ticker in Tickers.OfType<UpdateTicker>()) SafeTick(_ticker);

            foreach (var _tickerBase in Tickers.OfType<Ticker>())
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

        public IPlayer MakePlayer(PlayerData data)
        {
            var _ent = data.Memory.GetSelf();
            if (_ent == null)
                return null;
            if (_ent.GetWorldRepresentation().GetComponent<IPlayer>() != null)
                return null;
            var _player = _ent.GetWorldRepresentation().AddComponent<Player>();
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
            var _newPlayer = newOwner.GetWorldRepresentation().AddComponent<Player>();
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

        public void SubscribeToOnTickEntities<T>(IEntity.TickCallBack callBack) where T : IEntity
        {
            foreach (var _entity in Entities)
                if (_entity is T _breed)
                    _breed.OnTick += callBack;
        }

        //--Ent Stuff -----------------------------------------------------------------------------------------------------------------------------------
        public IThinker Brains => null;
        public IStatContainer Stats => new DummyStatContainer();

        public void DeInitializeBrains(IThinker thinker)
        {
        }

        public void InitializeBrains(IThinker thinker)
        {
        }

        public ICollection<IViewModel> View { get; } = new EmptyView().ToEnumerable().Cast<IViewModel>().ToList();
        public BlackBoard Memory { get; private set; }
        public Dictionary<Type, IEntityComponent> Components { get; } = new();

        public void AddEntComponent(IEntityComponent component)
        {
        }

        public bool HasInitialized { get; set; }
        public ITicker EntityTicker => EmptyTicker;
        public string ID => id;
        public event IEntity.TickCallBack OnTick;

        public World GetWorld()
        {
            return this;
        }

        public IEventBus Bus => GameBus;
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(23123132131321, 31232323231, 3132331321111));
        public Bounds CachedBounds => Bounds;
        public IVector EyeVector => new V3();

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

        [Inject(Id = IVideoGame.GAME_BUS_NAME)]
        public IEventBus GameBus;

        [Inject] public IGameStateHandler GameState { get; set; }
        [Inject] public ICursorStateHandler CursorState { get; set; }
        [Inject] public IAssetFactory AssetFactory { get; set; }
        public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem { get; }

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
        public List<IEntity> Children => Entities;

        public ISaveData Save()
        {
            return new GenericSaveData();
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
            _internal.Transition(this);
        }

        public GameplayTagContainer GameplayTag
        {
            get => GameplayTagContainer.Empty;
            set { }
        }

        public void ResetToDefault()
        {
            Debug.Log($"[World]: The world was reset");
            loadWorldTask.Cancel();
            Tickers.ForEach((@base => @base.ResetToDefault()));
            Entities.Clear();
            players.Clear();
            HasInitialized = false;
            shutDowned = false;
        }

        public T Create<T>() where T : Component, IEntity
        {
            return Create<T>(Vector3.zero.ToGeneric());
        }

        public T Create<T>(IVector pos) where T : Component, IEntity
        {
            return Create<T>(typeof(T).Name, pos);
        }

        public T Create<T>(string name, IVector pos) where T : Component, IEntity
        {
            return Create<T>(name, (entity => entity.Pos(pos)));
        }

        public T Create<T>(string name, Action<T> process) where T : Component, IEntity
        {
            var _obj = new GameObject(name);
            var _ent = _obj.AddComponent<T>();
            process.Invoke(_ent);
            AssetFactory.Create(_obj);
            return _ent;
        }

        public async UniTask<T> Clone<T>(T ent) where T : IEntity //--TODO make this better
        {
            var clone = await GameObject.InstantiateAsync(ent.GetWorldRepresentation());
            AssetFactory.Create(clone.FirstOrDefault());
            return clone.FirstOrDefault()!.GetComponent<T>();
        }


        public IEntity Create()
        {
            return Create<EmptyEntity>();
        }

        public IEntity Create(IVector param)
        {
            return Create<EmptyEntity>(param);
        }

        public IEntity Create(string param1, IVector param2)
        {
            return Create<EmptyEntity>(param1, param2);
        }
    }

    [Serializable]
    public class WorldEntityFactory<T> : IAsyncFactory<T> where T : Component, IEntity
    {
        [Inject] private World world;
        [Inject] private IAssetManager manager;
        [SerializeField] private AssetReferenceT<T> reff;

        public async UniTask<T> Create()
        {
            if (reff == null)
                return world.Create<T>();
            if (reff.editorAsset is IGamePoolableBase _poolable)
            {
                return await manager.Spawn<T>(_poolable.Info());
            }

            return await manager.Spawn<T>(new GameAssetInfo(reff.RuntimeKey.ToString()));
        }
    }
}