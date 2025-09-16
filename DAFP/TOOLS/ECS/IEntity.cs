using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.EventBus;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using NUnit.Framework;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable, IGameObjectProvider, IAuthor, INameable
    {
        public Dictionary<Type, IEntityComponent> Components { get; }
        public ISerializationService GetConfigService() => new ConfigSerializationService();

        public ISerializer GetConfigSerializer() => new ConfigSerializer();

        // public ISerializationService GetSaveService() => new ConfigSerializationService();
        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker { get; }
        public void RemovePet(IOwnable pet);
        public void AddPet(IOwnable pet);
        public string ID { get; }

        public delegate void TickCallBack(IEntity ent);

        public event TickCallBack OnTick;
        public World GetWorld();
    }
}