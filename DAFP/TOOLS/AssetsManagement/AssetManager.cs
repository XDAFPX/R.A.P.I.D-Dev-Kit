using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DAFP.GAME.Essential;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BuiltIn;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Zenject;
using Component = UnityEngine.Component;

namespace DAFP.GAME.Assets
{
    public abstract class AssetManager : IInitializable, IAssetManager
    {
        protected readonly IAssetFactory.DefaultAssetFactory defaultAssetFactory;

        [Inject]
        public AssetManager([Inject] IAssetFactory.DefaultAssetFactory defaultAssetFactory)
        {
            this.defaultAssetFactory = defaultAssetFactory;
        }

        private HashSet<IAssetPoolBase> _pools;


        public void Initialize()
        {
            _pools = GetPools();
        }

        protected abstract HashSet<IAssetPoolBase> GetPools();

        private T register_pool<T>(T based) where T : IAssetPoolBase
        {
            _pools.Add(based);
            return based;
        }

        public void Despawn(GameObject obj)
        {
            IPoolComponentProvider _provider = obj.GetComponent<IPoolComponentProvider>();
            if (_provider == null)
                return;


            if (ReleaseIGamePoolable(_provider)) return;

            GameObject.Destroy(obj);
        }

        public async void Spawn<T>(string prefix, string uName, Action<T> onComplete) where T : IGamePoolableBase
        {
            foreach (var _pool in _pools)
            {
                if (_pool.Prefix == prefix)
                {
                    var _result = await _pool.Get(uName, FormatAddress(prefix + "." + uName));
                    if (_result == null)
                    {
                        Debug.LogError("Critical Error. Result==null!");
                        continue;
                    }

                    if (_result is T _typedObj)
                    {
                        onComplete?.Invoke(_typedObj);
                        break;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Type mismatch when spawning asset {uName} in {prefix}. Expected : {typeof(T).FullName} got {_result.GetType().FullName}");
                    }
                }
            }
        }

        public async void Spawn(string adress, Action<GameObject> onComplete)
        {
            foreach (var _pool in _pools)
            {
                if (_pool.Prefix == GetPrefix(adress))
                {
                    var _result = await _pool.GetAsGameObject(GetUName(adress), adress);
                    onComplete.Invoke(_result);
                    return;
                }
            }

            Addressables.InstantiateAsync(adress);
        }

        public IEntity SpawnEmptyEntity(Vector3 pos)
        {
            var obj = new GameObject("EntEmpty");
            obj.transform.position = pos;
            return obj.AddEmptyEntity(defaultAssetFactory);
        }

        public bool ReleaseIGamePoolable(IPoolComponentProvider provider)
        {
            foreach (var _pool in _pools)
            {
                if (_pool.GetPoolType().IsAssignableFrom(provider.Self().GetType()))
                {
                    _pool.ReleaseGeneric(provider.Self());
                    return true;
                }
            }

            return false;
        }


        public string GetUName(string input)
        {
            bool _containsPrefix = false;
            foreach (var _prefix in AssetPrefixes.Values)
            {
                if (input.Contains(_prefix))
                {
                    _containsPrefix = true;
                    break;
                }
            }

            if (!_containsPrefix)
            {
                return input;
            }

            input = FormatAddress(input);
            string[] _parts = input.Split('.');
            return FormatUName(input.Replace("GAME.Assets.", "").Replace(_parts[2], ""));
        }

        public static string GetPrefix(string input)
        {
            input = FormatAddress(input);
            string[] _parts = input.Split('.');
            return _parts[2];
        }

        public static string FormatUName(string input)
        {
            if (!string.IsNullOrEmpty(input) && input[0] == '.')
            {
                input = input.Remove(0, 1);
            }

            return input;
        }

        public async void Spawn<T>(string uName, Action<T> onComplete) where T : IGamePoolableBase
        {
            int _deepness = 5;
            var _t = typeof(T);
            while ((_t != null && !AssetPrefixes.ContainsKey(_t)))
            {
                _deepness--;
                _t = _t.BaseType;
            }

            var _prefix = _t != null ? AssetPrefixes[_t] : default;
            uName = FormatUName(uName);
            foreach (var _pool in _pools)
            {
                if (_pool.Prefix == _prefix)
                {
                    var _result = await _pool.Get(uName, FormatAddress(_prefix + "." + uName));
                    if (_result is T _typedObj)
                    {
                        onComplete?.Invoke(_typedObj);
                        break;
                    }
                    else
                    {
                        Debug.LogError($"Type mismatch when spawning asset {uName} in {_prefix}");
                    }
                }
            }
        }

        // public async void SpawnEffect<T>(string _Uname, Action<T> onComplete) where T : Effect
        // {
        //     Effect Eresult = await EFFECTS_POOL.Get(_Uname, "GAME.Assets.Effects." + _Uname) as Effect;
        //     if (Eresult is T typedResult)
        //     {
        //         onComplete?.Invoke(typedResult);
        //     }
        //     else
        //     {
        //         if (Eresult != null)
        //             Debug.LogWarning(
        //                 $"Effect '{_Uname}' is not of expected type {Eresult.GetType().Name} (SEARCH NAME:{_Uname} , FOUND NAME:{Eresult.UName})");
        //         else
        //             Debug.LogWarning($"Effect '{_Uname}' not found! (?????????)");
        //     }
        // }

        public const string ASSETS_PREFIX = "GAME.Assets.";

        public static string FormatAddress(string input)
        {
            if (input[^1].Equals('.'))
                input.Remove(input.Length - 1);
            if (input[0].Equals('.'))
                input.Remove(0);

            if (!input.StartsWith(ASSETS_PREFIX))
                input = input.Insert(0, ASSETS_PREFIX);
            input = input.Replace("..", ".");
            input = input.Replace("...", ".");
            return input;
        }

        public abstract Dictionary<Type, string> AssetPrefixes { get; }

        public static async Task<bool> AddressExists(string address)
        {
            AsyncOperationHandle<IList<IResourceLocation>> _handle = Addressables.LoadResourceLocationsAsync(address);
            await _handle.Task;

            bool _exists = _handle.Status == AsyncOperationStatus.Succeeded && _handle.Result.Count > 0;
            Addressables.Release(_handle);

            return _exists;
        }
    }
}