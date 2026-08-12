using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Serialization;
using NRandom;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    public interface IWorldTransition
    {
        void Init(int sceneIndex);
    }

    internal interface IWorldTransitionInternal : IWorldTransition
    {
        UniTask Transition(World world);
    }


    public class SaveWorldTransition : WorldTransition
    {
        [Inject] private ISaveSystem saveSystem;
        [Inject] private IRandom randomSys;

        public override UniTask Transition(World world)
        {
            var _serializationService = new SaveSerializationDataService();
            var _serializer = new EntityDefaultSerializer();
            var _metaSerializer = new SaveMetaSerializer(world, randomSys);

            saveSystem.SaveAll(_serializationService, _serializer, _metaSerializer, 0);
            world.ResetToDefault();
            // saveSystem.TryChangeCurrentScene(_serializationService, _metaSerializer, SceneIndex, 0); TODO
            saveSystem.LoadAll(_serializationService, _serializer, _metaSerializer, 0);
            return UniTask.CompletedTask;
        }
    }

    public class AsyncWorldTransition : IWorldTransitionInternal
    {
        protected int SceneIndex;


        public void Init(int sceneIndex)
        {
            SceneIndex = sceneIndex;
        }

        public virtual async UniTask Transition(World world)
        {
            world.ResetToDefault();
            await SceneManager.LoadSceneAsync(SceneIndex, LoadSceneMode.Single);
        }
    }

    public class WorldTransition : IWorldTransitionInternal
    {
        protected int SceneIndex;


        public void Init(int sceneIndex)
        {
            SceneIndex = sceneIndex;
        }

        public virtual UniTask Transition(World world)
        {
            world.ResetToDefault();
            SceneManager.LoadScene(SceneIndex, LoadSceneMode.Single);
            return UniTask.CompletedTask;
        }
    }
}