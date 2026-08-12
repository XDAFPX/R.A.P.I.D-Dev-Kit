using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using MessagePipe;
using ModestTree;
using Optional.Unsafe;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    internal sealed class SceneBootStrap : IInitializable, IDisposable // scans each scene load for the entities that were not prepared
    {
        [Inject] private IObjectCreatePreparer createPreparer;
        [Inject] private ISubscriber<OnSceneLoadEvent> sceneE;

        private IDisposable sub;
        void IInitializable.Initialize() // -- ALWAYS the last to init
        {
            sub = sceneE.Subscribe((@event => ProcessScene(@event.Scene)));
        }

        public void Dispose()
        {
            sub.Dispose();
        }


        private void ProcessScene(Scene scene)
        {
            var roots = scan_scene(scene).ToArray();
            prepare_objects(roots).Forget();
        }

        private IEnumerable<GameObject> scan_scene(Scene scene)
        {
            if (!scene.IsValid())
                return Enumerable.Empty<GameObject>();

            var roots = scene.GetRootGameObjects();
            return roots;
        }

        private async UniTask prepare_objects(IEnumerable<GameObject> objects)
        {
            foreach (var _gameObject in objects)
            {
                if (_gameObject == null) continue;

                await createPreparer.Create(Adam.CreationInfo.Scene(), _gameObject);
            }
        }
    }
}