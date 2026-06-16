using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class ExecuteAllSpawnPointsSys :  IListener<OnWorldInitEvent>
    {
        [Inject] private World world;
        [Inject] private ISpawnPointManager[] managers;

        public void React(in OnWorldInitEvent e)
        {
            managers.ForEach((manager => manager.ManageAll()));
        }
    }
}