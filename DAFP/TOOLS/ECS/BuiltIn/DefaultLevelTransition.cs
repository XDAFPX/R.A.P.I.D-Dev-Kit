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
    public class LevelTransition<T> : EmptyEntity where T : IWorldTransition
    {
        [Inject] private ISaveSystem saveSystem;
        [Inject] private IRandom randomSys;
        [Inject] private DiContainer container;


        public void Transition(string sceneName)
        {
            Transition(GameUtils.GetSceneIndexByName(sceneName));
        }

        public void Transition(int sceneIndex)
        {
            var _tr = container.Instantiate<T>();
            _tr.Init(sceneIndex);
            World.Transition(_tr);
        }
    }
}