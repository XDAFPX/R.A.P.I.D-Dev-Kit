using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Services;
using NUnit.Framework;
using RapidLib.DAFP.TOOLS.Common;
using RapidLib.DAFP.TOOLS.Common.Utill;
using TNRD;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using AdamUtils = RapidLib.DAFP.TOOLS.Common.Utill.AdamUtils;

namespace DAFP.TOOLS.ECS.Environment.Factories
{
    // [CreateAssetMenu(fileName = nameof(DefaultEntityFactory), menuName = "R.A.P.I.D/Factory/" + nameof(DefaultEntityFactory))]
    [Serializable]
    public class DefaultEntityFactory :  IAsyncFactory<IEnumerable<IEntity>>, IAsyncFactory<IEntity>
    {
        [Inject] protected Adam Adam;
        protected int Ammount;
        protected AddressableEntry Reference;

        async UniTask<IEnumerable<IEntity>> IAsyncFactory<IEnumerable<IEntity>>.Create()
        {
            var _l = new List<IEntity>();
            for (int i = 0; i < Ammount; i++)
            {
                IEntity _ent = await Create();
                _l.Add(_ent);
            }

            return _l;
        }

        public async UniTask<IEntity> Create()
        {
            return await Adam.Create<IEntity>(new GameAssetInfo(Reference.Address));
        }
    }
}