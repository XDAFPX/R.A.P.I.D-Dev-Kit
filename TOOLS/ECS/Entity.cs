using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS
{
    public abstract class Entity : MonoBehaviour, IEntity, IDisposable, IPet, IRandomizeable
    {
        [InspectorName("RandomizingFrom0To100")] [Range(0, 100)] [SerializeField]
        private float Variety = 10;

        public HashSet<IEntityComponent> Components { get; }

        private void Awake()
        {
            ((IEntity)this).Initialize();
        }

        private void GatherComponents()
        {
            foreach (var component in
                     gameObject.GetComponents<IEntityComponent>())
            {
                AddEntComponent(component);
            }
        }


        public T GetEntComponent<T>() where T : EntityComponent
        {
            return Components.FirstOrDefault(component => component is T) as T;
        }

        public void AddEntComponent(IEntityComponent component)
        {
            Components.Add(component);
            component.Register(this);
        }

        protected abstract ITicker<IEntity> EntityTicker { get; }

        void IEntity.Initialize()
        {
            World.RegisterEntity(this, EntityTicker);
            GatherComponents();
            foreach (IEntityComponent _component in Components)
            {
                _component.OnInitializeInternal();
            }

            HasInitialized = true;
        }

        public void Tick()
        {
            foreach (var component in Components)
            {
                component.Tick();
            }
        }

        public bool HasInitialized { get; set; }
        public ITicker<IEntity> ITicker { get; set; }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }


        protected HashSet<IPet> Pets = new HashSet<IPet>();

        public virtual void AddPet(IPet pet)
        {
            Pets.Add(pet);
        }

        public virtual void RemovePet(IPet pet)
        {
            Pets.Remove(pet);
        }

        public List<Entity> Owners { get; } = new List<Entity>();

        public Entity GetCurrentOwner()
        {
            return Owners.LastOrDefault();
        }

        public Entity GetExOwner()
        {
            return Owners.Count >= 2 ? Owners[^2] : null;
        }

        public void ChangeOwner(Entity newOwner)
        {
            if (GetCurrentOwner() == null)
            {
                if (newOwner == null) return;
                newOwner.AddPet(this);
                Owners.Add(newOwner);
            }
            else if (GetCurrentOwner() != null)
            {
                GetCurrentOwner().RemovePet(this);
                Owners.Add(newOwner);
                if (newOwner != null)
                    newOwner.AddPet(this);
            }
        }

        public void Randomize(float margin01)
        {
            foreach (var component in Components)
            {
                if (component is IRandomizeable randomizeable)
                    randomizeable.Randomize(margin01);
            }
        }
    }
}