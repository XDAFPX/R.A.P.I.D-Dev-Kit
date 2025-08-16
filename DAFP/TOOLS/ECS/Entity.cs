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

        public HashSet<IEntityComponent> Components { get; } = new HashSet<IEntityComponent>();


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

        public abstract ITicker<IEntity> EntityTicker { get; }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            World.RegisterEntity(this, EntityTicker);
            GatherComponents();
            foreach (IEntityComponent _component in Components)
            {
                _component.Initialize();
            }

            InitializeInternal();
            HasInitialized = true;
        }

        public void OnStart()
        {
            if (Variety != 0)
                Randomize(Variety / 100);
            foreach (IEntityComponent _entityComponent in Components)
            {
                _entityComponent.OnStartInternal();
            }
        }


        public void Tick()
        {
            foreach (var component in Components)
            {
                component.Tick();
            }

            TickInternal();
        }

        protected abstract void TickInternal();
        protected abstract void InitializeInternal();
        public bool HasInitialized { get; set; }

        public GameObject GetWorldRepresentation()
        {
            return gameObject;
        }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }


        protected HashSet<IOwnable> Pets = new HashSet<IOwnable>();

        public virtual void AddPet(IOwnable pet)
        {
            Pets.Add(pet);
        }

        public virtual void RemovePet(IOwnable pet)
        {
            Pets.Remove(pet);
        }

        public List<IEntity> Owners { get; } = new List<IEntity>();

        public IEntity GetCurrentOwner()
        {
            return Owners.LastOrDefault();
        }

        public IEntity GetExOwner()
        {
            return Owners.Count >= 2 ? Owners[^2] : null;
        }

        public void ChangeOwner(IEntity newOwner)
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