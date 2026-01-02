using DAFP.TOOLS.ECS;
using UnityEngine;
using Zenject;

namespace DAFP.GAME.Assets
{
    public interface IAssetFactory
    {
        public GameObject InjectD(GameObject gameObject);

        public class DefaultAssetFactory : IAssetFactory
        {
            private readonly DiContainer diContainer;

            [Inject]
            public DefaultAssetFactory([Inject] DiContainer diContainer)
            {
                this.diContainer = diContainer;
            }

            public GameObject InjectD(GameObject gameObject)
            {
                if (gameObject.TryGetComponent<IEntity>(out IEntity _entity))
                {
                    diContainer.Inject(_entity);
                    if (_entity is Entity ent)
                    {
                        ent.FlagAsInstantiated();
                    }
                }

                return gameObject;
            }
        }
    }
}