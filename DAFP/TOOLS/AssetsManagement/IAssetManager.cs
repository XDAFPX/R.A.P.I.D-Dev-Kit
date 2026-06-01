using System;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.AssetManagement
{
    public interface IAssetManager : IInitializable
    {
        IAssetFactory Factory { get; }
        bool Release(IGamePoolableBase provider);
        void Despawn(GameObject obj);

        UniTask<T> Spawn<T>(GameAssetInfo info) where T : Component;
    }
    
}