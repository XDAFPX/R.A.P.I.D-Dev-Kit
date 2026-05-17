using System;
using System.Collections.Generic;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Serialization;
using NUnit.Framework;

namespace DAFP.TOOLS.ECS
{
    public interface IPlayer : INameable , IEntityComponent, ISavable
    {
        public PlayerData Data { get;  }
        public IEntity Body { get; }
    }
    [System.Serializable]
    public struct PlayerData 
    {
        public bool IsLocal;
        public BlackBoard Memory;

        public PlayerData(bool isLocal, BlackBoard memory)
        {
            IsLocal = isLocal;
            Memory = memory;
        }

        public PlayerData SetData(BlackBoard @new)
        {
            return new PlayerData(IsLocal, @new);
        }
    }
}