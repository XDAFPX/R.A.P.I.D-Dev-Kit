using UnityEngine;

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

        public void Register(IEntity entity)
        {
            Host = entity;
        }


        public T GetEntComponent<T>() where T : EntityComponent
        {
            return Host.GetEntComponent<T>();
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