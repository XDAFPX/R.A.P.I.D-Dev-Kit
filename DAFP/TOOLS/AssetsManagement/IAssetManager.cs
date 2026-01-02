using System;
using UnityEngine;
using Zenject;

namespace DAFP.GAME.Assets
{
    // Interface extracted from AssetManager for spawning and despawning common stuff
    public interface IAssetManager : IInitializable
    {
        // Despawn an object, returning it to its pool if possible
        public bool ReleaseIGamePoolable(IPoolComponentProvider provider);
        void Despawn(GameObject obj);

        // Spawn by explicit prefix and unique name
        void Spawn<T>(string prefix, string uName, Action<T> onComplete) where T : IGamePoolableBase;

        // Spawn by full address, returns raw GameObject
        void Spawn(string address, Action<GameObject> onComplete);

        // Spawn by type-inferred prefix map
        void Spawn<T>(string uName, Action<T> onComplete) where T : IGamePoolableBase;

// Spawn by explicit prefix + unique name + position
        void Spawn<T>(string prefix, string uName, Vector3 position, Action<T> onComplete)
            where T : MonoBehaviour, IGamePoolableBase
        {
            Spawn<T>(prefix, uName, (obj) =>
            {
                var t = obj.transform;
                t.position = position;
                onComplete(obj);
            });
        }

// Spawn raw GameObject by address + position
        void Spawn(string address, Vector3 position, Action<GameObject> onComplete)
        {
            Spawn(address, obj =>
            {
                var t = obj.transform;
                t.position = position;
                onComplete(obj);
            });
        }

// Spawn by type-inferred prefix + unique name + position
        void Spawn<T>(string uName, Vector3 position, Action<T> onComplete)
            where T : MonoBehaviour, IGamePoolableBase
        {
            Spawn<T>(uName, obj =>
            {
                var t = obj.transform;
                t.position = position;
                onComplete(obj);
            });
        }
    }
}