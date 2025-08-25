using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using NUnit.Framework;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable, IGameObjectProvider
    {
        public Dictionary<Type, IEntityComponent> Components { get; } 

        public bool HasInitialized { get; set; }
        public ITicker<IEntity> EntityTicker { get; }
        public void RemovePet(IOwnable pet);
        public void AddPet(IOwnable pet);
        public string ID { get; }

        public delegate void TickCallBack(IEntity ent);

        public event TickCallBack OnTick;
    }
}