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
                new SaveMetaSerializer(World, randomSys), 0);
            saveSystem.TryChangeCurrentScene(new SaveSerializationService(), new SaveMetaSerializer(World, randomSys), SceneIndex,
                0);
            saveSystem.LoadAll(new SaveSerializationService(), new SaveSerializer(), new SaveMetaSerializer(World, randomSys),
                null, 0);
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