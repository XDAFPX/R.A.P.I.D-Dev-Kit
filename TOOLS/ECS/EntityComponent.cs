using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public abstract class EntityComponent : MonoBehaviour, IEntityComponent
    {
        protected Entity Host;
        protected abstract void OnTick();
        protected abstract void OnInitialize();
        protected abstract void OnStart();

        public void Tick()
        {
            OnTick();
        }

        public void Register(Entity entity)
        {
            Host = entity;
        }

        public void OnInitializeInternal()
        {
            OnInitialize();
        }

        public void OnStartInternal()
        {
            OnStart();
        }
    }
}