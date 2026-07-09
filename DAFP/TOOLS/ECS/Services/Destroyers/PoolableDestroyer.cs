using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Components;
using FluentResults;
using Optional.Unsafe;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.Diagnostics;
using Zenject;

namespace DAFP.TOOLS.ECS.Services.Destroyers
{
    internal class PoolableDestroyer : IDestroyer
    {
        [Inject] private IAssetManagerLogic logic;

        public UniTask<Adam.DestructionInfo> Destroy(object value)
        {
            var resolve = GameUtils.ResolveAs<IGamePoolableBase>(value);

            if (!resolve.HasValue) return UniTask.FromResult(Adam.DestructionInfo.Failure());


            var _info = GameUtils.ResolveAs<GameObject>(resolve.ValueOrDefault()).ValueOrDefault()
                            ?.GetComponent<CreationInfoContainer>()?.Info ??
                        Adam.CreationInfo.Scene();
            GameUtils.ResolveAs<GameObject>(resolve.ValueOrDefault()).TryGetValue(out var _val);
                CreationInfoContainer.AbsenceFrom(_val);
            resolve.ValueOrFailure().ResetToDefault();
            logic.Release(resolve.ValueOrFailure());
            return UniTask.FromResult(Adam.DestructionInfo.Success(_info));
        }

        // public static GameObject ResolveToGameObject(object obj)
        // {
        //     return obj switch
        //     {
        //         IEntity ent => ent.GetWorldRepresentation(),
        //         Component comp => comp.gameObject,
        //         GameObject go => go,
        //         _ => null
        //     };
        // }


        public int Priority { get; set; } = -100;
    }
}