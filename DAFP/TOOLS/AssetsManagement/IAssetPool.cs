using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace DAFP.GAME.Assets
{
    public interface IAssetPool<T,TP> : IAssetPoolBase where TP : Component,IGameGamePoolable<TP>  where T : IGameGamePoolable<TP> 
    {
        int Count { get; }



        //PooledObject<TP> Get(out T v);

        void Release(T element);

        
    }

    public interface IAssetPoolBase
    {
        void Clear();
        Type GetPoolType();
        void ReleaseGeneric(Component element);
        internal string Prefix { get; }
        Task<IGamePoolableBase> Get(string uName,string adress);
        Task<GameObject> GetAsGameObject(string uName,string adress);
        
    }
}