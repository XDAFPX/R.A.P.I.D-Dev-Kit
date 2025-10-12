using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.EventBus;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using NUnit.Framework;
using UnityEngine;


namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable, IGameObjectProvider, IAuthor, INameable,IOwner<IEntity>,ISavable,IDebugDrawable,ISwitchable
    {
        public Dictionary<Type, IEntityComponent> Components { get; }

        public void AddEntComponent(IEntityComponent component);
        public ISerializationService GetConfigService() => new ConfigSerializationService();

        public ISerializer<IEntity> GetConfigSerializer() => new ConfigSerializer();

        // public ISerializationService GetSaveService() => new ConfigSerializationService();
        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker { get; }
        public string ID { get; }

        public delegate void TickCallBack(IEntity ent);

        public event TickCallBack OnTick;
        public World GetWorld();
        public Bounds Bounds { get; }

        public NonEmptyList<IViewModel> View { get; }
    }
}