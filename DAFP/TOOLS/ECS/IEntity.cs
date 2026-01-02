using System;
using System.Collections.Generic;
using BandoWare.GameplayTags;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.EventBus;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.ViewModel;
using NUnit.Framework;
using UnityEngine;
using UnityEventBus;
using Zenject;


namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable, IGameObjectProvider, IAuthor, INameable, IOwner<IEntity>, ISavable,
        IDebugDrawable, ISwitchable, IHaveGameplayTag
    {
        public IThinker Brains { get; }
        public StatContainer Stats { get; }

        public void DeInitializeBrains(IThinker thinker);

        public void InitializeBrains(IThinker thinker);

        public NonEmptyList<IViewModel> View { get; }

        public BlackBoard Memory { get; }
        public Dictionary<Type, IEntityComponent> Components { get; }

        public void AddEntComponent(IEntityComponent component);

        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker { get; }
        public string ID { get; }

        public delegate void TickCallBack(IEntity ent);

        public event TickCallBack OnTick;
        public World GetWorld();
        public IEventBus Bus { get; }
        public Bounds Bounds { get; }
        public IVectorBase EyeVector { get; }
        public void RecalculateBounds();
        public void Remove(EntityRemovalReason removalReason);
        public void BroadcastEvent<T>(T @event) where T : struct;
    }
}