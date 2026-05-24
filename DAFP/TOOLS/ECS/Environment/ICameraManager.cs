using System.Collections;
using System.Collections.Generic;
using BandoWare.GameplayTags;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Serialization;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment
{
    public interface ICameraManager : ISavable, IOwnerOf<IGameCamera>,IInitializable,ITickable
    {
        public IEnumerable<IEntity> Subjects { get; }

        public void AddSubject(IEntity ent);
        public void RemoveSubject(IEntity ent);
    }

    public interface IGameCamera : IPetOf<ICameraManager, IGameCamera>,IHaveGameplayTag
    {
        
        public IStat<float> Zoom { get; }
        public void Shake(ShakeSettings settings);
    }
}