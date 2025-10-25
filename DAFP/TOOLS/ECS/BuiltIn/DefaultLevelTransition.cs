using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using NRandom;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class DefaultLevelTransition : Entity
    {
        [Inject] private ISaveSystem saveSystem;
        [Inject] private IRandom RandomSys;

        public static void Transition(
            string sceneName,
            ISaveSystem saveSystem,
            World world,
            IRandom randomSys)
        {
            int sceneIndex = GetSceneIndexByName(sceneName);
            Transition(sceneIndex, saveSystem, world, randomSys);
        }

        public static void Transition(
            int sceneIndex,
            ISaveSystem saveSystem,
            World world,
            IRandom randomSys)
        {
            var serializationService = new SaveSerializationService();
            var serializer = new SaveSerializer();
            var metaSerializer = new SaveMetaSerializer(world, randomSys);

            saveSystem.SaveAll(serializationService, serializer, metaSerializer, 0);
            saveSystem.TryChangeCurrentScene(serializationService, metaSerializer, sceneIndex, 0);
            saveSystem.LoadAll(serializationService, serializer, metaSerializer, null, 0);
        }

        public void Transition(string scenename)
        {
            Transition(scenename,SaveSystem,World,RandomSys);
        }

        public void Transition(int SceneIndex)
        {
            Transition(SceneIndex,SaveSystem,World,RandomSys);
        }

        public static int GetSceneIndexByName(string sceneName)
        {
            int count = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1; // not found
        }

        public override NonEmptyList<IViewModel> SetupView()
        {
            return new NonEmptyList<IViewModel>(new EmptyView());
        }

        public override ITicker<IEntity> EntityTicker { get; } = null;

        protected override void TickInternal()
        {
        }

        protected override void InitializeInternal()
        {
        }
    }
}