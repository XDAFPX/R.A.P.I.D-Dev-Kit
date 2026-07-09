using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.ECS.BuiltIn;
using FluentResults;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace DAFP.TOOLS.ECS.Services.Creators
{
    internal class CatchAllCreator : ICreator
    {
        private ICreator[] others;

        [Inject]
        public void Initialize(ICreator[] all) =>
            others = all
                .Where(c => c is not CatchAllCreator)
                .OrderByDescending(c => c.Priority)
                .ToArray();


        public async UniTask<Result<object>> Create(Adam.CreationInfo info)
        {
            var _resolved = resolve(info.WantedType);
            if (_resolved == null)
                return fail(info);

            return await run_chain(info.WithType(_resolved));
        }

        private static Type resolve(Type wanted) => wanted switch
        {
            _ when wanted == typeof(IEntity) => typeof(EmptyEntity),
            // _ when wanted == typeof(ICreature) => typeof(BaseCreature),
            // _ when wanted == typeof(IFoodNode) => typeof(BaseFoodNode),

            _ when wanted == typeof(GameObject) => typeof(Transform),
            _ when wanted == typeof(Object) => typeof(GameObject),

            _ when wanted.IsInterface => null, // unknown interface, bail
            _ when wanted.IsAbstract => null, // unknown abstract, bail

            _ => null
        };

        private async UniTask<Result<object>> run_chain(Adam.CreationInfo info)
        {
            foreach (var _creator in others)
            {
                var _res = await _creator.Create(info);
                if (_res.IsSuccess) return _res;
            }

            return fail(info);
        }

        private static Result<object> fail(Adam.CreationInfo info) =>
            Result.Fail<object>(new Error(
                $"[{nameof(CatchAllCreator)}] :: No handler resolved for type '{info.WantedType?.Name}' " +
                $"with info: {info.asset_info}"));

        public int Priority { get; set; } = -1000;
    }
}