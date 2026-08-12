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
    public class EntLevelTransition : EmptyEntity
    {
        [Inject] private ISaveSystem saveSystem;
        [Inject] private IRandom randomSys;
        [Inject] private IWorldTransition transition;


        public void Transition(string sceneName)
        {
            Transition(GameUtils.GetSceneIndexByName(sceneName));
        }

        public void Transition(int sceneIndex)
        {
            var _tr = transition;
            _tr.Init(sceneIndex);
            World.Transition(_tr);
        }
    }
}