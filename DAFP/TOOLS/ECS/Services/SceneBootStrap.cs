using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
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
    internal sealed class SceneBootStrap : IInitializable // scans each scene load for the entities that were not prepared
    {
        [Inject] private IObjectCreatePreparer createPreparer;


        void IInitializable.Initialize()
        {
            var _all = scan_scene(SceneManager.GetActiveScene()).ToArray();
            prepare_objects(_all).Forget();
        }


        private IEnumerable<GameObject> scan_scene(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            return roots;
        }

        private IEnumerable<IEntity> entities(IEnumerable<GameObject> source)
        {
            return source.Select((o => GameUtils.ResolveAs<IEntity>(o).ValueOrDefault())).ClearOfNulls();
        }

        private IEnumerable<IEntity> entities_with_no_parent(IEnumerable<IEntity> source)
        {
            return source.Where((entity => ((IPetOwnerTreeOf<IEntity>)entity).GetCurrentOwner() == null));
        }

        private async UniTask resolve_tree(IEntity entity, Action<IEntity> e)
        {
            e.Invoke(entity);
            if (entity.Children.IsEmpty())
            {
                return;
            }

            foreach (var _entityChild in entity.Children)
            {
                await resolve_tree(_entityChild, e);
            }
        }

        private async UniTask prepare_objects(IEnumerable<GameObject> objects)
        {
            foreach (var _gameObject in objects)
            {
                await createPreparer.Create(Adam.CreationInfo.Scene(), _gameObject);
            }
        }
    }
}