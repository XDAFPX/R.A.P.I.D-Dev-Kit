using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Basic.Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Zenject;
using Component = UnityEngine.Component;

namespace DAFP.TOOLS.AssetManagement
{
    public abstract class AssetManager : IInitializable, IAssetManager, INameable,IAssetManagerLogic
    {
        protected readonly IAssetFactory AssetFactory;

        private HashSet<IAssetPoolBase> _pools;
        public string Name { get; set; } = nameof(AssetManager);

        [Inject]
        public AssetManager([Inject] IAssetFactory assetFactory)
        {
            this.AssetFactory = assetFactory;
        }

        public IAssetFactory Factory => AssetFactory;


        public void Initialize()
        {
            _pools = GetPools();
        }


        public async UniTask<T> Spawn<T>(GameAssetInfo info) where T : Component
        {
            var _poolable = await spawn_poolable<T>(info);
            if (_poolable != null) return _poolable;
            var _go = await AssetFactory.Create(info.FullAddress);
            return _go.GetComponent<T>();
        }


        public void Despawn(GameObject obj)
        {
            if (obj.TryGetComponent(out IGamePoolableBase _poolable))
            {
                Release(_poolable);
                return;
            }
            
            GameObject.Destroy(obj);
        }


        public bool Release(IGamePoolableBase provider)
        {
            foreach (var _pool in _pools)
            {
                if (_pool.Prefix != provider.Prefix) continue;
                _pool.ReleaseGeneric(provider);
                return true;
            }

            return false;
        }

        protected abstract HashSet<IAssetPoolBase> GetPools();

        private async UniTask<T> spawn_poolable<T>(GameAssetInfo info) where T : Component
        {
            foreach (var _pool in _pools)
            {
                if (_pool.Prefix != info.Prefix) continue;
                var _result = await _pool.Get(info);

                switch (_result)
                {
                    case null:
                        Debug.Log( $"Critical error! Pool({_pool.Prefix}) failed.");
                        continue;
                    case T _comp:
                    {
                        _result.OnSpawn();

                        return _comp;
                    }
                    default:
                        Debug.LogError(
                            $"Type mismatch when spawning asset {info.UName} in {info.Prefix}. Expected : {typeof(T).FullName} got {_result.GetType().FullName}");
                        break;
                }
            }

            return null;
        }


        // public static async Task<bool> AddressExists(string address)
        // {
        //     AsyncOperationHandle<IList<IResourceLocation>> _handle = Addressables.LoadResourceLocationsAsync(address);
        //     await _handle.Task;
        //
        //     bool _exists = _handle.Status == AsyncOperationStatus.Succeeded && _handle.Result.Count > 0;
        //     Addressables.Release(_handle);
        //
        //     return _exists;
        // }
    }
}