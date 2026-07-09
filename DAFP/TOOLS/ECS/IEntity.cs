using System;
using System.Collections.Generic;
using BandoWare.GameplayTags;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Modifiers.Pegs;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.ViewModel;
using NUnit.Framework;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using Zenject;


namespace DAFP.TOOLS.ECS
{
    public interface IEntity : ITickable, IGameObjectProvider,  INameable, IPetOwnerTreeOf<IEntity>, ISavable,
        IDebugDrawable, ISwitchable, IHaveGameplayTag, IOwnerOf<IViewModel>, IHaveStats,
        IOwnerOf<IStatModifierBase>, IOwnerOf<PegModifier>, IOwnerOf<IEntityAccessory>, IDirectionProvider
    {
        public string ID { get; }
        
        

        public bool HasInitialized { get;  }

        public IThinker Brains { get;  }
        public ICollection<IViewModel> View { get; }

        public ITicker EntityTicker { get; }
        public BlackBoard Memory { get; }


        public World GetWorld();
        public Bounds Bounds { get; }
        public Bounds CachedBounds { get; }
    }

    public interface IDirectionProvider
    {
        public IVector EyeVector { get; }
    }
}