using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DAFP.GAME.Assets
{
    public class AssetPool<T, TP> : IDisposable, IAssetPool<T, TP>
        where T : IGameGamePoolable<TP> where TP : Component, IGameGamePoolable<TP>
    {
        public Type GetPoolType()
        {
            return typeof(TP);
        }

        public void ReleaseGeneric(Component element)
        {
            if (element is T _poolable)
            {
                Release(_poolable);
            }
        }

        string IAssetPoolBase.Prefix => prefix;

        internal List<T> Assets;
        private readonly int maxSize;
        internal T FreshRelease;

        public List<T> GetMembers()
        {
            return Assets;
        }

        private string prefix;

        private readonly IAssetFactory factory;
        //private string prefix;

        public AssetPool(string prefix, IAssetFactory factory,
            bool collectionCheck = true,
            int defaultCapacity = 100,
            int maxSize = 500)
        {
            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            this.Assets = new List<T>(defaultCapacity);
            this.maxSize = maxSize;
            this.prefix = prefix;
            this.factory = factory;
        }


        public int Count => Assets.Count;

        public async Task<IGamePoolableBase> Get(string uName, string adress)
        {
            if (just_released_the_same_thing(uName, out var _poolable)) return _poolable;


            return await spawn_or_find_existing(uName, adress);


            bool just_released_the_same_thing(string uName, out TP poolable)
            {
                if (FreshRelease != null && FreshRelease.UName == uName) // fresh release check
                {
                    var _release = FreshRelease.Get();
                    FreshRelease = default(T);
                    poolable = _release;
                    return true;
                }

                poolable = null;
                return false;
            }

            async Task<TP> spawn_or_find_existing(string s, string adress)
            {
                int _possibleIndex = Assets.FindIndex((poolable => poolable.UName == s));
                if (_possibleIndex != -1) //
                {
                    var _obj = Assets[_possibleIndex];
                    Assets.RemoveAt(_possibleIndex);
                    return _obj.Get();
                }

                else
                {
                    var _handle = Addressables.InstantiateAsync(adress);
                    await _handle.Task;
                    return factory.InjectD(_handle.Result).GetComponent<TP>();
                }
            }
        }

        public async Task<GameObject> GetAsGameObject(string uName, string adress)
        {
            if (just_released_the_same_thing(uName, out var _poolable)) return _poolable.gameObject;


            return await spawn_or_find_existing(uName, adress);


            bool just_released_the_same_thing(string uName, out TP poolable)
            {
                if (FreshRelease != null && FreshRelease.UName == uName) // fresh release check
                {
                    var _release = FreshRelease.Get();
                    FreshRelease = default(T);
                    poolable = _release;
                    return true;
                }

                poolable = null;
                return false;
            }

            async Task<GameObject> spawn_or_find_existing(string s, string adress)
            {
                int _possibleIndex = Assets.FindIndex((poolable => poolable.UName == s));
                if (_possibleIndex != -1) //
                {
                    var _obj = Assets[_possibleIndex];
                    Assets.RemoveAt(_possibleIndex);
                    return _obj.Get().gameObject;
                }

                else
                {
                    var _handle = Addressables.InstantiateAsync(AssetManager.FormatAddress(adress));
                    await _handle.Task;
                    return factory.InjectD(_handle.Result);
                }
            }

            ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(T element)
        {
            if (HasElement(element))
            {
                return;
            }

            if (FreshRelease == null)
            {
                element.ResetObj();
                FreshRelease = element;
                return;
            }


            if (this.Count < this.maxSize)
            {
                element.ResetObj();
                this.Assets.Add(element);
            }
            else
            {
                dispose_of(element);
            }
        }

        private void dispose_of(T obj)
        {
            if (obj == null)
                return;
            if (!Addressables.ReleaseInstance((obj as Component).gameObject))
            {
                obj.Dispose();
            }
        }

        public void Clear()
        {
            foreach (T _obj in this.Assets)
                dispose_of(_obj);

            this.Assets.Clear();
        }


        public void Dispose() => this.Clear();

        public bool HasElement(T element)
        {
            return this.Assets.Contains(element) || (FreshRelease != null && FreshRelease.Equals(element));
        }
    }
}