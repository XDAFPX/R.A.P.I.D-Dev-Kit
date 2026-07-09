using System;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Components;
using Optional.Unsafe;

namespace DAFP.TOOLS.ECS.Services.Destroyers
{
    internal  class DisposableDestroyer : IDestroyer
    {
        public async UniTask<Adam.DestructionInfo> Destroy(object value)
        {
            if (value is not IDisposable _disposable) return Adam.DestructionInfo.Failure();

            var _info = GameUtils.ResolveAs<CreationInfoContainer>(value).ValueOrDefault()?.Info ??
                        Adam.CreationInfo.Scene();
            
            
            _disposable.Dispose();
            
            
            return Adam.DestructionInfo.Success(_info);
        }

        public int Priority { get; set; } = -800;
    }
}