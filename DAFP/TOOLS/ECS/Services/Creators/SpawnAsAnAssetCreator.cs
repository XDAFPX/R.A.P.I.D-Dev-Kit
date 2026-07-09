using System;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.ECS.Components;
using FluentResults;
using Zenject;

namespace DAFP.TOOLS.ECS.Services.Creators
{
    internal class SpawnAsAnAssetCreator : ICreator
    {
        [Inject] private IAssetManagerLogic manager;
        [Inject] private IAssetFactory factory;

        public async UniTask<Result<object>> Create(Adam.CreationInfo param1)
        {
            if (param1.asset_info is not GameAssetInfo _info || param1.newObject)
                return fail(param1);
            try
            {
                var res = await manager.Spawn<Entity>(_info);
                return Result.Ok(res);
            }
            catch (Exception e)
            {
                return fail(param1);
            }
        }

        private static Result<object> fail(Adam.CreationInfo param1)
        {
            return Result.Fail<object>(
                new Error($"[SpawnFromAssetCreator] :: Failed to create the asset with info : {param1.asset_info}"));
        }

        public int Priority { get; set; } = -100;
    }
}