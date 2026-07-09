using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Components;
using Optional.Unsafe;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Services.Destroyers
{
    internal class UnityObjectDestroyer : IDestroyer
    {
        public UniTask<Adam.DestructionInfo> Destroy(object value)
        {
            if (value is not Object o) return UniTask.FromResult(Adam.DestructionInfo.Failure());
            // if (go.TryGetComponent<IEntity>(out _))
            //     return UniTask.FromResult(Adam.DestructionInfo.Failure());

            var _info = GameUtils.ResolveAs<CreationInfoContainer>(o).ValueOrDefault()?.Info ??
                        Adam.CreationInfo.Scene();
            Object.Destroy(o);
            return UniTask.FromResult(Adam.DestructionInfo.Success(_info));
        }

        public int Priority { get; set; } = -500;
    }
}