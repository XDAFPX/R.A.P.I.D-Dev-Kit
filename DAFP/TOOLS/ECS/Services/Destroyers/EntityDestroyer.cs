using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Components;
using Optional.Unsafe;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services.Destroyers
{
    internal class EntityDestroyer : IDestroyer
    {
        [Inject] private ThinkerDestroyer destroyer;
        public async UniTask<Adam.DestructionInfo> Destroy(object value)
        {
            var _resolve = GameUtils.ResolveAs<IEntity>(value);
            if (!_resolve.TryGetValue(out var _ent)) return Adam.DestructionInfo.Failure();


            var _info = GameUtils.ResolveAs<CreationInfoContainer>(value).ValueOrDefault()?.Info ??
                        Adam.CreationInfo.Scene();
            
            //everything else is handled via events 
            if(_ent is IResetable _resetable)
                _resetable.ResetToDefault();
            await destroyer.Destroy(_ent.Brains);
            GameObject.Destroy(_ent.GetWorldRepresentation());
            return Adam.DestructionInfo.Success(_info);
        }


        public int Priority { get; set; } = -111;
    }
}