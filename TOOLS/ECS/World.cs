using System;
using System.Collections;
using System.Collections.Generic;
using DAFP.GAME.Essential;
using JetBrains.Annotations;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public class World : Manager<World>
    {
        protected static readonly HashSet<IEntity> ENTITIES = new HashSet<IEntity>();
        protected static readonly HashSet<ITicker<IEntity>> TICKERS = new HashSet<ITicker<IEntity>>();


        public static void RegisterTicker([NotNull] ITicker<IEntity> ticker)
        {
            TICKERS.Add(ticker);
        }

        public static void RegisterEntity([NotNull] IEntity ent, [NotNull] ITicker<IEntity> ticker)
        {
            if (ENTITIES.Contains(ent))
                return;
            ticker.Subscribed.Add(ent);
            ENTITIES.Add(ent);

            Debug.Log(
                $"Registered Creature... Name: {ent.GetType().Name} , WorldName: {(ent is Entity _entity ? _entity.name : "NoName")} ");
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

        protected void Start()
        {
            foreach (var Tticker in TICKERS)
            {
                StartCoroutine(Cycle(Tticker.Tick, 1 / Tticker.UpdatesPerSecond));
            }
        }

    }
}