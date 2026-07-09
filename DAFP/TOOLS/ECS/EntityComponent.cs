using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Services;
using NRandom;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    [RequireComponent(typeof(Entity))]
    public abstract class EntityComponent : MonoBehaviour, IEntityComponent
    {
        [Inject] protected World World;
        [Inject] protected Adam Adam;

        [Inject] protected IRandom RandomSys;

        public IEntity Host { get; private set; }
        protected abstract void OnTick();
        protected abstract void OnInitialize();

        public void Initialize()
        {
            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);
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

        void IEntityComponent.Register(IEntity entity)
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