using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Components;
using DAFP.TOOLS.Injection;
using MessagePipe;
using RapidLib.DAFP.TOOLS.Common;
using TripleA.Utils.Extensions;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    internal interface IObjectCreatePreparer : IAsyncFactory<Adam.CreationInfo, object, object>
    //--the after injection and preparement of an object
    {
    }

    [Preserve]
    internal class InitialObjectCreatePreparer : IObjectCreatePreparer
    {
        [Inject] private DiContainer injector;
        [Inject] private IAsyncPublisher<OnObjectCreatedAsyncEvent> creationEvent;
        [Inject] private IPublisher<OnObjectRegisterEvent> registerEvent;
        [Inject] private IPublisher<OnEntityReadyToInitializeEvent> readyToInitEvent;

        public async UniTask<object> Create(Adam.CreationInfo param1, object param2)
        {
            var _prepared = new List<object>();
            prepare_recursive(param1, param2, _prepared);

            // registration happens synchronously for the whole tree first
            foreach (var _obj in _prepared)
            {
                registerEvent.Publish(new OnObjectRegisterEvent(_obj));
            }

            // then async creation events fire for each, tree already fully registered
            foreach (var _obj in _prepared)
            {
                await creationEvent.PublishAsync(new OnObjectCreatedAsyncEvent(_obj));
            }

            // internal bus
            foreach (var _obj in _prepared)
            {
                if (GameUtils.ResolveAs<IEntity>(_obj).TryGetValue(out var _val))
                    readyToInitEvent.Publish(new(_val));
            }

            return param2;
        }

        private void prepare_recursive(Adam.CreationInfo param1, object param2, List<object> collected)
        {
            if (param2 == null) return;

            // 1. Single GameObject resolution & cache
            GameObject go = param2 as GameObject;
            if (go == null && GameUtils.ResolveAs<GameObject>(param2).TryGetValue(out var resolvedGo))
            {
                go = resolvedGo;
            }

            // 2. Immediate RunnableContext Guard
            if (param2 is RunnableContext || (go != null && go.TryGetComponent<RunnableContext>(out _)))
            {
                return; // Hard stop: Do not inject, do not collect, do not traverse children
            }

            // 3. Process GameObject-specific setup
            if (go != null)
            {
                injector.InjectGameObject(go);
                CreationInfoContainer.EnsureOn(go, param1);

                if (go.TryGetComponent<IEntity>(out var _entity))
                {
                    scan_children(go.transform, _entity);
                }
            }

            // 4. Inject object dependencies & initialize mods
            injector.Inject(param2);
            if (param2 is IMod _mod)
            {
                _mod.Initialize();
            }
            collected.Add(param2);

            // 5. Traverse children using cached GameObject reference
            if (go != null)
            {
                foreach (var _transform in go.transform.Children())
                {
                    prepare_recursive(param1, _transform.gameObject, collected);
                }
            }
        }

        private void scan_children(Transform parent, IEntity owner)
        {
            foreach (Transform _child in parent)
            {
                // Never register pets that belong to a RunnableContext subtree
                if (_child.TryGetComponent<RunnableContext>(out _))
                    continue;

                if (_child.TryGetComponent<IEntity>(out var _entity))
                {
                    ((IPetOwnerTreeOf<IEntity>)owner).AddPet(_entity);
                }
                else
                {
                    scan_children(_child, owner);
                }
            }
        }
    }
}
