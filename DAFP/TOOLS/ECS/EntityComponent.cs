using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Services;
using NRandom;
using UnityEngine;
using UnityEventBus;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    // [RequireComponent(typeof(Entity))]
    public abstract class EntityComponent : MonoBehaviour, IEntityComponent, ISubscriber
    {
        [Inject] protected World World;

        [Inject] protected IRandom RandomSys;

        public IEntity Host;
        protected abstract void OnTick();
        protected abstract void OnInitialize();

        public void Initialize()
        {
            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);
            Host.Bus.Subscribe(this);
            // World.Bus.Subscribe(this);
            OnInitialize();
        }

        private void OnValidate()
        {
            Host = GetComponent<IEntity>();
        }


        public void Tick()
        {
            if (enabled && Host.HasInitialized)
                OnTick();
        }

        public virtual ITicker EntityComponentTicker => Host.EntityTicker;

        public void Register(IEntity entity)
        {
            Host = entity;
            if (EntityComponentTicker != Host.EntityTicker)
                Host.GetWorld().RegisterCustomComponentTicker(this, EntityComponentTicker);
        }

        public virtual IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return Array.Empty<IDebugDrawer>();
        }


        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }

        private void OnDrawGizmos()
        {
            //DO NOIT OVERRIDE
        }
    }
}