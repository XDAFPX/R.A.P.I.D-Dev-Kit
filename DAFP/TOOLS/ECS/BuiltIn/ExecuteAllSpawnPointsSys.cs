using System;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Services;
using MessagePipe;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class ExecuteAllSpawnPointsSys : IInitializable, IDisposable
    {
        [Inject] private ISpawnPointManager[] managers;
        [Inject] private ISubscriber<OnWorldDecisionEvent> init;
        private IDisposable sub;

        void IInitializable.Initialize() =>
            sub = init.Subscribe(x =>
            {
                managers.ForEach((manager => manager.Resolve()));
            });

        void IDisposable.Dispose() => sub.Dispose();
    }
}