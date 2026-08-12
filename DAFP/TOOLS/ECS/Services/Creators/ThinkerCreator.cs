using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.Thinkers;
using FluentResults;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services.Creators
{
    public class ThinkerCreator : ICreator
    {
        [Inject] private DiContainer injector;

        public UniTask<Result<object>> Create(Adam.CreationInfo param1)
        {
            if (param1.asset_info is not ThinkerCreationInfo _creationInfo) return UniTask.FromResult(fail(param1));
            if (_creationInfo.Original is IThinkerLogic { HasWoken: true })
                return UniTask.FromResult<Result<object>>(Result.Ok(_creationInfo.Original));
            var _clone = deep_clone(_creationInfo.Original);
            deep_inject(_clone);
            if (_clone is IThinkerLogic _logic)
                _logic.HasWoken = true;
            return UniTask.FromResult<Result<object>>(Result.Ok(_clone));
        }


        private static Result<object> fail(Adam.CreationInfo param1)
        {
            return Result.Fail<object>(
                new Error(
                    $"[{nameof(ThinkerCreator)}] :: Failed to create the asset with info : {param1.asset_info}"));
        }


        private IThinker deep_clone(IThinker original)
        {
            #if UNITY_EDITOR
            if (original is Brain { EditMode: true })
                return original;
            #endif
            if (original is not ScriptableObject _so)
                return original;
            return (IThinker)_so.DeepClone();
        }

        public int Priority { get; set; } = -99;

        private void deep_inject(IThinker original)
        {
            injector.Inject(original);
            original.AllPets().ForEach(injector.Inject);
        }
    }
}