using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DAFP.GAME.Assets;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.GlobalState;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    public abstract class World : MonoBehaviour, IService

    {
        public static readonly Ticker<IEntity> EMPTY_TICKER = new(0, new HashSet<IGlobalGameState>());
        public readonly List<IEntity> ENTITIES = new();
        protected readonly List<ITickerBase> TICKERS = new();

        public IEntity SpawnEmptyEntity(Vector3 pos)
        {
            var obj = new GameObject("EntEmpty");
            obj.transform.position = pos;
            return obj.AddEmptyEntity(AssetFactory);
        }

        public void RegisterTicker([NotNull] ITickerBase ticker)
        {
            if (TICKERS.Contains(ticker))
                return;

            TICKERS.Add(ticker);
            TICKERS.Sort();
        }

        public void SubscribeToOnTickEntities<T>(IEntity.TickCallBack callBack) where T : IEntity
        {
            foreach (var _entity in ENTITIES)
                if (_entity is T breed)
                    breed.OnTick += callBack;
        }

        public void RegisterEntity(IEntity ent, ITicker<IEntity> ticker)
        {
            if (ent == null || ticker == null)
                return;
            if (ENTITIES.Contains(ent))
                return;
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);
            ENTITIES.Add(ent);
            if (ent is IEntityOwnable _ownable)
                AddPet(_ownable);

            Debug.Log(
                $"Registered Entity... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
        }

        public void RemoveEntity([NotNull] IEntity ent)
        {
            try
            {
                ENTITIES.Remove(ent);
                ent.EntityTicker.Subscribed.Remove(ent);
                foreach (var _entityComponent in ent.Components)
                    if (_entityComponent.Value.EntityComponentTicker != ent.EntityTicker)
                        _entityComponent.Value.EntityComponentTicker.Remove(_entityComponent.Value);

                if (ent is IEntityOwnable _ownable)
                    RemovePet(_ownable);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unregistered entity : {ent.Name} was removed :: {e} ");
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

        private void Awake()
        {
            Initialize();
        }


        private void FixedUpdate()
        {
            FixedTick();
        }


        private void Update()
        {
            Tick();
        }


        public void Initialize()
        {
            id = Guid.NewGuid().ToString();
            ENTITIES.Clear();
            foreach (var _tickerBase in TICKERS) _tickerBase.ResetToDefault();

            TICKERS.Clear();
            HasInitialized = true;
        }

        public void FixedTick()
        {
            foreach (var _ticker in TICKERS.OfType<FixedUpdateTicker<IEntity>>()) SafeTick(_ticker);

            foreach (var _ticker in TICKERS.OfType<FixedUpdateTicker<IEntityComponent>>()) SafeTick(_ticker);
        }

        public void SafeTick(ITickerBase ticker)
        {
            if (ticker.IsAllowedToTick(GameState.Current()))
                ticker.Tick();
        }


        public void Tick()
        {
            foreach (var _ticker in TICKERS.OfType<UpdateTicker<IEntity>>()) SafeTick(_ticker);

            foreach (var _ticker in TICKERS.OfType<UpdateTicker<IEntityComponent>>()) SafeTick(_ticker);

            foreach (var _tickerBase in TICKERS.OfType<Ticker<ITickable>>())
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


        public bool HasInitialized { get; set; }


        protected HashSet<IEntityOwnable> Pets = new();
        private string id;


        public virtual void AddPet(IEntityOwnable pet)
        {
            Pets.Add(pet);
        }

        public string ID => id;

        public virtual void RemovePet(IEntityOwnable pet)
        {
            Pets.Remove(pet);
        }

        [Inject] public IGlobalGameStateHandler GameState { get; set; }
        [Inject] public IGlobalCursorStateHandler CursorState { get; set; }
        [Inject] public IAssetFactory.DefaultAssetFactory AssetFactory { get; set; }
    }
}