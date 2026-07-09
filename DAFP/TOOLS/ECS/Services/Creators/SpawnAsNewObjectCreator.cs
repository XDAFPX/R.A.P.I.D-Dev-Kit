using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.ECS.Components;
using FluentResults;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services.Creators
{
    internal class SpawnAsNewObjectCreator : ICreator
    {
        // [Inject] private World world;
        [Inject] private IAssetFactory factory;

        public UniTask<Result<object>> Create(Adam.CreationInfo param1)
        {
            if (!param1.newObject)
                return UniTask.FromResult(fail(param1));
            if (!typeof(Component).IsAssignableFrom(param1.WantedType))
                return UniTask.FromResult(fail(param1));
            var _name = param1.ObjName ?? "Cain";
            var _type = param1.WantedType;
            var _created = new GameObject(_name);
            var _comp = _created.AddComponent(_type);
            return UniTask.FromResult(Result.Ok<object>(_comp));
        }

        private static Result<object> fail(Adam.CreationInfo param1)
        {
            return Result.Fail<object>(
                new Error(
                    $"[{nameof(SpawnAsNewObjectCreator)}] :: Failed to create the asset with info : {param1.asset_info}"));
        }

        public int Priority { get; set; } = -100;
    }
}