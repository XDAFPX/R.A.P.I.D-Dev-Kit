using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Zenject;

namespace DAFP.TOOLS.AssetManagement
{
    public class AssetPool<T, TP> :  IAssetPool<T, TP>
        where T : IGamePoolable<TP> where TP : Component, IGamePoolable<TP> //TODO figure out DO I really need Generics here and in Game Poolablej
    {
        public AssetPool(string prefix,
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


        private string prefix;

        [Inject] private IAssetFactory factory;
        public int Count => Assets.Count;
        internal List<T> Assets;
        private readonly int maxSize;
        internal T FreshRelease;

        string IAssetPoolBase.Prefix => prefix;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<IGamePoolableBase> Get(GameAssetInfo info)
        {
            if (just_released_the_same_thing(info.UName, out var _poolable)) return _poolable;


            return await spawn_or_find_existing(info.UName, info.FullAddress);


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
                    var _obj = await factory.Create(adress);
                    return _obj.GetComponent<TP>();
                }
            }
        }


        public void ReleaseGeneric(IGamePoolableBase element)
        {
            if (element is T _poolable)
            {
                Release(_poolable);
            }
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

            if (obj is Component cp && cp.gameObject == null)
                return;


            if (obj is Component c)
            {
                GameObject.Destroy(c.gameObject); //cant inject adam since you know
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

        public Type GetPoolType()
        {
            return typeof(TP);
        }

        public List<T> GetMembers()
        {
            return Assets;
        }
    }
}