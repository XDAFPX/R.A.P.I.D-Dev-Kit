using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Thinkers;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Services.Destroyers
{
    internal class ThinkerDestroyer : IDestroyer
    {
        public async UniTask<Adam.DestructionInfo> Destroy(object value)
        {
            if (value is not IThinkerLogic { HasWoken: true } _thinker) return Adam.DestructionInfo.Failure();
            if (!_thinker.Name.Contains("Clone")) return Adam.DestructionInfo.Failure();
            switch (value)
            {
                case ScriptableObject _so:
                    _so.DeepDestroy();
                    break;
                case Object _obj:
                    Object.Destroy(_obj);
                    break;
            }

            return Adam.DestructionInfo.Success(Adam.CreationInfo.Scene());
        }

        public int Priority { get; set; } = -99;
    }
}