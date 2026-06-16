using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.ECS.Environment;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class InfoPlayerStart : EntFactory
    {
        [field: SerializeField]
        public SerializableInterface<IAsyncFactory<IEnumerable<IPlayer>>> PlayerFactory { get; set; }

        protected override SerializableInterface<IAsyncFactory<IEnumerable<IEntity>>> Factory { get; set; }
        [Inject(Id = "PlayerManager")]protected override ISpawnPointManager Manager { get; set; }

        public override async UniTask<IEnumerable<IEntity>> Create()
        {
            Factory = new SerializableInterface<IAsyncFactory<IEnumerable<IEntity>>>(new Bridge(PlayerFactory.Value));
            return await base.Create();
        }

        private class Bridge : IAsyncFactory<IEnumerable<IEntity>>
        {
            private readonly IAsyncFactory<IEnumerable<IPlayer>> factory;
            [Inject] private DiContainer container;
            public Bridge(IAsyncFactory<IEnumerable<IPlayer>> factory)
            {
                this.factory = factory;
            }

            public async UniTask<IEnumerable<IEntity>> Create()
            {
                container.Inject(factory);
                var _res = await factory.Create();
                return _res.Select((player => player.Body));
            }
        }
    }
}