using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Services;
using PixelRouge.CsharpExtensionMethods;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{

    public class InfoEntityStart : EntFactory
    {
        [field: SerializeField]
        protected override SerializableInterface<IAsyncFactory<IEnumerable<IEntity>>> Factory { get; set; }

        [Inject]protected override ISpawnPointManager Manager { get; set; }
    }

    public abstract class EntFactory : EmptyEntity, ISpawnPoint,IPetOf<ISpawnPointManager,ISpawnPoint>
    {
        protected abstract SerializableInterface<IAsyncFactory<IEnumerable<IEntity>>> Factory { get; set; }

        protected abstract ISpawnPointManager Manager { get; set; }
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            ((IOwnedBy<ISpawnPointManager>)this).ChangeOwner(Manager);
        }


        public virtual async UniTask<IEnumerable<IEntity>> Create()
        {
            Injector.Inject(Factory.Value);
            var res = await Factory.Value.Create();

            var _enumerable = res as IEntity[] ?? res.ToArray();
            _enumerable.ForEach((entity => entity.Pos(Bounds.Point(Rng))));

            return _enumerable;
        }

        protected override Bounds CalculateBounds()
        {
            return GameUtils.CalculateCombinedBounds(this);
        }

        async UniTask<IEntity> IAsyncFactory<IEntity>.Create()
        {
            var res = await Create();
            return res.FirstOrDefault();
        }

        List<ISpawnPointManager> IPetOf<ISpawnPointManager, ISpawnPoint>.Owners => new ();
    }
}