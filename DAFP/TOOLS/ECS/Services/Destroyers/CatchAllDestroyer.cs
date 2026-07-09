using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.ECS.Services.Creators;
using FluentResults;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services.Destroyers
{
    internal class CatchAllDestroyer : IDestroyer
    {
        private IDestroyer[] others;

        [Inject]
        public void Initialize(IDestroyer[] all) =>
            others = all
                .Where(c => c is not CatchAllDestroyer)
                .OrderByDescending(c => c.Priority)
                .ToArray();

        public async UniTask<Adam.DestructionInfo> Destroy(object value)
        {
            return await run_chain(resolve(value) ?? value);
        }

        private object resolve(object obj)
        {
            return obj switch
            {
                GameObject go => go.GetComponent<IEntity>(),
                _ => null
            };
        }

        private async UniTask<Adam.DestructionInfo> run_chain(object val)
        {
            foreach (var _destroyer in others)
            {
                var _res = await _destroyer.Destroy(val);
                if (_res.Result.IsSuccess) return _res;
            }

            return Adam.DestructionInfo.Failure();
        }

        public int Priority { get; set; } = -1000;
    }
}