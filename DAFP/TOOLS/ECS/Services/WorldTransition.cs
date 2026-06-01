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
        void Transition(World world);
    }


    public class SaveWorldTransition : WorldTransition
    {
        [Inject] private ISaveSystem saveSystem;
        [Inject] private IRandom randomSys;

        public override void Transition(World world)
        {
            var _serializationService = new SaveSerializationService();
            var _serializer = new SaveSerializer();
            var _metaSerializer = new SaveMetaSerializer(world, randomSys);

            saveSystem.SaveAll(_serializationService, _serializer, _metaSerializer, 0);
            world.ResetToDefault();
            saveSystem.TryChangeCurrentScene(_serializationService, _metaSerializer, SceneIndex, 0);
            saveSystem.LoadAll(_serializationService, _serializer, _metaSerializer, null, 0);
        }
    }

    public class WorldTransition : IWorldTransitionInternal
    {
        protected int SceneIndex;


        public void Init(int sceneIndex)
        {
            SceneIndex = sceneIndex;
        }

        public virtual void Transition(World world)
        {

            world.ResetToDefault();
            SceneManager.LoadScene(SceneIndex, LoadSceneMode.Single);

        }
    }
}