using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using NRandom;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class DefaultLevelTransition : Entity
    {
        [Inject] private ISaveSystem saveSystem;
        [Inject] private IRandom randomSys;

        // public void Transition(string Scenename)
        // {
        //     var index = SceneManager.GetSceneByName(Scenename).buildIndex;
        //     saveSystem.TryChangeCurrentScene(new SaveSerializationService(), new SaveMetaSerializer(world), index);
        //     saveSystem.LoadAll(new SaveSerializationService(), new SaveSerializer(), new SaveMetaSerializer(world),
        //         null);
        // }

        public void Transition(int SceneIndex)
        {
            saveSystem.SaveAll(new SaveSerializationService(), new SaveSerializer(),
                new SaveMetaSerializer(world, randomSys), 0);
            saveSystem.TryChangeCurrentScene(new SaveSerializationService(), new SaveMetaSerializer(world, randomSys), SceneIndex,
                0);
            saveSystem.LoadAll(new SaveSerializationService(), new SaveSerializer(), new SaveMetaSerializer(world, randomSys),
                null, 0);
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