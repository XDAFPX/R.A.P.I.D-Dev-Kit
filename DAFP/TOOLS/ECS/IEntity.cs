using System.Collections.Generic;
using DAFP.TOOLS.Common;
using NUnit.Framework;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable, IGameObjectProvider
    {
        public HashSet<IEntityComponent> Components { get; }

        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker { get; }
        public T GetEntComponent<T>() where T : EntityComponent;
        public void AddEntComponent(IEntityComponent component);
        public void RemovePet(IOwnable pet);
        public void AddPet(IOwnable pet);
    }
}