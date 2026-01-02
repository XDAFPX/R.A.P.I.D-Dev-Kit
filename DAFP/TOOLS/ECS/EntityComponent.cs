using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using UnityEngine;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS
{
    // [RequireComponent(typeof(Entity))]
    public abstract class EntityComponent : MonoBehaviour, IEntityComponent
    {
        public IEntity Host;
        protected abstract void OnTick();
        protected abstract void OnInitialize();
        protected abstract void OnStart();

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
        {if(enabled)
            OnTick();
        }

        public virtual ITickerBase EntityComponentTicker => Host.EntityTicker;

        public void Register(IEntity entity)
        {
            Host = entity;
            if (EntityComponentTicker != Host.EntityTicker &&
                EntityComponentTicker is ITicker<IEntityComponent> customTicker)
                Host.GetWorld().RegisterCustomComponentTicker(this, customTicker);
        }

        public virtual IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return Array.Empty<IDebugDrawer>();
        }

        public void OnStartInternal()
        {
            OnStart();
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