using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS
{
    // [RequireComponent(typeof(Entity))]
    public abstract class EntityComponent : MonoBehaviour, IEntityComponent
    {
        protected IEntity Host;
        protected abstract void OnTick();
        protected abstract void OnInitialize();
        protected abstract void OnStart();

        public void Initialize()
        {
            AnimationNameCacheInitializer.InitializeCaches(this);
            GetComponentCacheInitializer.InitializeCaches(this);
            OnInitialize();
        }

        void ITickable.OnStart()
        {
            OnStartInternal();
        }

        public void Tick()
        {
            OnTick();
        }

        public virtual ITickerBase EntityComponentTicker
        {
            get => Host.EntityTicker;
        }

        public void Register(IEntity entity)
        {
            Host = entity;
            if (EntityComponentTicker != Host.EntityTicker &&
                EntityComponentTicker is ITicker<IEntityComponent> customTicker)
                World.RegisterCustomComponentTicker(this, customTicker);
        }


        public void OnStartInternal()
        {
            OnStart();
        }

        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }
    }
}