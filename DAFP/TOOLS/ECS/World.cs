using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DAFP.GAME.Essential;
using DAFP.TOOLS.Common;
using JetBrains.Annotations;
using UnityEngine;

// TODO make entity sound events 
namespace DAFP.TOOLS.ECS
{
    public abstract class World : Manager<World>
    {
        protected static readonly HashSet<IEntity> ENTITIES = new HashSet<IEntity>();
        protected static readonly HashSet<ITicker<IEntity>> TICKERS = new HashSet<ITicker<IEntity>>();

        public static ITicker<IEntity> FixedUpdateTicker = new Ticker<IEntity>(52);
        public static ITicker<IEntity> UpdateTicker = new UpdateTicker<IEntity>();

        public static void RegisterTicker([NotNull] ITicker<IEntity> ticker)
        {
            TICKERS.Add(ticker);
        }

        public static void RegisterEntity([NotNull] IEntity ent, [NotNull] ITicker<IEntity> ticker)
        {
            if (ENTITIES.Contains(ent))
                return;
            RegisterTicker(ticker);
            ticker.Subscribed.Add(ent);
            ENTITIES.Add(ent);

            Debug.Log(
                $"Registered Entity... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
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
        }

        protected void Start()
        {
            foreach (var Tticker in TICKERS)
            {
                Tticker.OnStart();
                if (!Mathf.Approximately(Tticker.UpdatesPerSecond, 52) && Tticker is not UpdateTicker<IEntity>)
                    StartCoroutine(Cycle(Tticker.Tick, Tticker.DeltaTime));
            }
        }
    }
}