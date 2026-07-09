using System.Collections;
using System.Collections.Generic;
using BandoWare.GameplayTags;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment
{
    public interface ICameraManager : ISavable, IOwnerOf<IGameCamera>,IInitializable,ITickable //--thing that delegates subjects
    {
        public UniTask Resolve();
    }

    public interface IGameCamera : IPetOf<ICameraManager, IGameCamera>,IHaveGameplayTag //--thing that has to keep up with multiple subjects 
    {
        
        public IStat<float> Zoom { get; }
        public void Shake(ShakeSettings settings);

        public void UpdateSubjects(IEnumerable<IEntity> subjects);
        
        public Rect Rect { get; set; }
        // public void OnSubjectsChanged(IEnumerable<IEntity> subjects); --shit
    }
}