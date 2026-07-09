using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;

namespace DAFP.TOOLS.AssetManagement
{
    public interface IAsyncDestroyer
    {
    }

    public interface IAsyncDestroyer<in TValue> : IAsyncFactory
    {
        UniTask Destroy(TValue value);
    }

    public interface IAsyncDestroyer<TParam, in TValue> : IAsyncFactory
    {
        UniTask<TParam> Destroy(TValue value);
    }

    public interface IAsyncFactory
    {
    }

    public interface IAsyncFactory<TValue> : IAsyncFactory
    {
        UniTask<TValue> Create();
    }

    public interface IAsyncFactory<in TParam1, TValue> : IAsyncFactory
    {
        UniTask<TValue> Create(TParam1 param1);
    }

    public interface IAsyncFactory<in TParam1, in TParam2, TValue> : IAsyncFactory
    {
        UniTask<TValue> Create(TParam1 param1, TParam2 param2);
    }

    public interface
        IAssetFactory : IAsyncFactory<string, GameObject>,
        ITickable //--initial creation of go by strign
    {
    }


    public class AssetFactory : IAssetFactory
    {
        private readonly DiContainer diContainer;
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> handles = new();
        private readonly Dictionary<string, float> lastUsed = new();
        private const float RELEASE_AFTER = 60f;

        [Inject]
        public AssetFactory(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }

        public void Tick()
        {
            var _toRelease = lastUsed
                .Where(kv => Time.time - kv.Value > RELEASE_AFTER)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var _address in _toRelease)
            {
                Addressables.Release(handles[_address]);
                handles.Remove(_address);
                lastUsed.Remove(_address);
            }
        }

        public async UniTask<GameObject> Create(string address)
        {
            if (!handles.TryGetValue(address, out var _handle))
            {
                _handle = Addressables.LoadAssetAsync<GameObject>(address);
                handles[address] = _handle;
            }

            lastUsed[address] = Time.time;
            var _prefab = await _handle.ToUniTask();
            return Object.Instantiate(_prefab);
        }
    }
}