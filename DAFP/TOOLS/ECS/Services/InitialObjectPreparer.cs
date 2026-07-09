using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Components;
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
            if (GameUtils.ResolveAs<GameObject>(param2).TryGetValue(out var _val))
            {
                if (_val.TryGetComponent<RunnableContext>(out var _context))
                {
                    Debug.LogWarning(
                        $"[{typeof(InitialObjectCreatePreparer)}] :: Almost injected a runnable context ({_context.GetType()}) whooops");
                    return; // skip this branch entirely, including its children
                }

                injector.InjectGameObject(_val);
                CreationInfoContainer.EnsureOn(_val, param1);
                if (_val.TryGetComponent<IEntity>(out var _entity))
                {
                    scan_children(_val.transform, _entity);
                }
            }

            injector.Inject(param2);
            collected.Add(param2);

            if (GameUtils.ResolveAs<GameObject>(param2).TryGetValue(out var _preparedObject))
            {
                foreach (var _transform in _preparedObject.transform.Children())
                {
                    prepare_recursive(param1, _transform.gameObject, collected);
                }
            }
        }

        private void scan_children(Transform parent, IEntity owner)
        {
            foreach (Transform _child in parent)
            {
                var _entity = _child.GetComponent<IEntity>();
                if (_entity != null)
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