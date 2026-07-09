using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using Optional.Unsafe;

namespace RapidLib.DAFP.TOOLS.Common.Utill
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public static class AdamUtils
    {
        // ── Internal helpers ──────────────────────────────────────────────────────

        private static T ResolveOrThrow<T>(object raw) where T : class
        {
            return GameUtils.ResolveAs<T>(raw)
                .ValueOrFailure($"[Adam] :: Created object '{raw}' could not be resolved as {typeof(T).Name}");
        }

        private static void ApplyTransform<T>(T obj, Action<Transform> apply) where T : class
        {
            var resolved = GameUtils.ResolveAs<Component>(obj);
            if (!resolved.HasValue)
                Debug.LogWarning(
                    $"[Adam] :: Placement was requested for type {typeof(T).Name} but it could not be resolved to a Component. Position/rotation will be ignored.");
            else
                resolved.MatchSome(c => apply(c.transform));
        }

        private static Vector3 to_vector3(this IVector v) => new(
            v.GetValueAtDimension(0) ?? 0f,
            v.GetValueAtDimension(1) ?? 0f,
            v.GetValueAtDimension(2) ?? 0f);

        private static Quaternion to_quaternion(this IVector v) =>
            Quaternion.Euler(
                v.GetValueAtDimension(0) ?? 0f,
                v.GetValueAtDimension(1) ?? 0f,
                v.GetValueAtDimension(2) ?? 0f);


        // ── Basic typed ───────────────────────────────────────────────────────────


        public static async UniTask<T> Create<T>(this Adam adam, Adam.CreationInfo info) where T : class
            => ResolveOrThrow<T>((T)await adam.Create(info));

        public static async UniTask<T> Create<T>(this Adam adam) where T : class
            => ResolveOrThrow<T>(await adam.Create(Adam.CreationInfo.New<T>()));

        public static async UniTask<T> Create<T>(this Adam adam, string name) where T : class
            => ResolveOrThrow<T>(await adam.Create(Adam.CreationInfo.New<T>(name)));

        public static async UniTask<T> Create<T>(this Adam adam, GameAssetInfo asset) where T : class
            => ResolveOrThrow<T>(await adam.Create(Adam.CreationInfo.Asset<T>(asset)));

        // ── Placement (Vector3) ───────────────────────────────────────────────────

        public static async UniTask<T> Create<T>(this Adam adam, Vector3 position) where T : class
        {
            var obj = await adam.Create<T>();
            ApplyTransform(obj, t => t.position = position);
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, Vector3 position, Quaternion rotation) where T : class
        {
            var obj = await adam.Create<T>();
            ApplyTransform(obj, t => t.SetPositionAndRotation(position, rotation));
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, Vector3 position, Quaternion rotation, Vector3 scale)
            where T : class
        {
            var obj = await adam.Create<T>();
            ApplyTransform(obj, t =>
            {
                t.SetPositionAndRotation(position, rotation);
                t.localScale = scale;
            });
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, Vector3 position, Quaternion rotation,
            Transform parent) where T : class
        {
            var obj = await adam.Create<T>();
            ApplyTransform(obj, t =>
            {
                t.SetParent(parent, false);
                t.SetLocalPositionAndRotation(position, rotation);
            });
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, Transform parent, bool worldPositionStays = false)
            where T : class
        {
            var obj = await adam.Create<T>();
            ApplyTransform(obj, t => t.SetParent(parent, worldPositionStays));
            return obj;
        }

        // ── Placement (IVector) ───────────────────────────────────────────────────

        public static async UniTask<T> Create<T>(this Adam adam, IVector position) where T : class
            => await adam.Create<T>(position.to_vector3());

        public static async UniTask<T> Create<T>(this Adam adam, IVector position, IVector rotation) where T : class
            => await adam.Create<T>(position.to_vector3(), rotation.to_quaternion());

        public static async UniTask<T> Create<T>(this Adam adam, IVector position, IVector rotation, IVector scale)
            where T : class
            => await adam.Create<T>(position.to_vector3(), rotation.to_quaternion(), scale.to_vector3());

        public static async UniTask<T> Create<T>(this Adam adam, IVector position, IVector rotation, Transform parent)
            where T : class
            => await adam.Create<T>(position.to_vector3(), rotation.to_quaternion(), parent);

        public static async UniTask<T> Create<T>(this Adam adam, IVector position, Quaternion rotation) where T : class
            => await adam.Create<T>(position.to_vector3(), rotation);

        public static async UniTask<T> Create<T>(this Adam adam, Vector3 position, IVector rotation) where T : class
            => await adam.Create<T>(position, rotation.to_quaternion());

        // ── Named + placement ─────────────────────────────────────────────────────

        public static async UniTask<T> Create<T>(this Adam adam, string name, Vector3 position) where T : class
        {
            var obj = await adam.Create<T>(name);
            ApplyTransform(obj, t => t.position = position);
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, string name, Vector3 position, Quaternion rotation)
            where T : class
        {
            var obj = await adam.Create<T>(name);
            ApplyTransform(obj, t => t.SetPositionAndRotation(position, rotation));
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, string name, IVector position) where T : class
            => await adam.Create<T>(name, position.to_vector3());

        public static async UniTask<T> Create<T>(this Adam adam, string name, IVector position, IVector rotation)
            where T : class
            => await adam.Create<T>(name, position.to_vector3(), rotation.to_quaternion());

        // ── Asset + placement ─────────────────────────────────────────────────────

        public static async UniTask<T> Create<T>(this Adam adam, GameAssetInfo asset, Vector3 position) where T : class
        {
            var obj = await adam.Create<T>(asset);
            ApplyTransform(obj, t => t.position = position);
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, GameAssetInfo asset, Vector3 position,
            Quaternion rotation) where T : class
        {
            var obj = await adam.Create<T>(asset);
            ApplyTransform(obj, t => t.SetPositionAndRotation(position, rotation));
            return obj;
        }

        public static async UniTask<T> Create<T>(this Adam adam, GameAssetInfo asset, IVector position) where T : class
            => await adam.Create<T>(asset, position.to_vector3());

        public static async UniTask<T> Create<T>(this Adam adam, GameAssetInfo asset, IVector position,
            IVector rotation) where T : class
            => await adam.Create<T>(asset, position.to_vector3(), rotation.to_quaternion());

        // ── Many (params / batch) ─────────────────────────────────────────────────

        public static async UniTask<T[]> Create<T>(this Adam adam, int count) where T : class
        {
            var results = new T[count];
            for (var i = 0; i < count; i++)
                results[i] = await adam.Create<T>();
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam, params Vector3[] positions) where T : class
        {
            var results = new T[positions.Length];
            for (var i = 0; i < positions.Length; i++)
                results[i] = await adam.Create<T>(positions[i]);
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam, params IVector[] positions) where T : class
        {
            var results = new T[positions.Length];
            for (var i = 0; i < positions.Length; i++)
                results[i] = await adam.Create<T>(positions[i].to_vector3());
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam,
            params (Vector3 position, Quaternion rotation)[] transforms) where T : class
        {
            var results = new T[transforms.Length];
            for (var i = 0; i < transforms.Length; i++)
                results[i] = await adam.Create<T>(transforms[i].position, transforms[i].rotation);
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam,
            params (IVector position, IVector rotation)[] transforms) where T : class
        {
            var results = new T[transforms.Length];
            for (var i = 0; i < transforms.Length; i++)
                results[i] = await adam.Create<T>(transforms[i].position, transforms[i].rotation);
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam, int count, Func<int, Vector3> position)
            where T : class
        {
            var results = new T[count];
            for (var i = 0; i < count; i++)
                results[i] = await adam.Create<T>(position(i));
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam, int count, Func<int, IVector> position)
            where T : class
        {
            var results = new T[count];
            for (var i = 0; i < count; i++)
                results[i] = await adam.Create<T>(position(i));
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam, int count, Func<int, Vector3> position,
            Func<int, Quaternion> rotation) where T : class
        {
            var results = new T[count];
            for (var i = 0; i < count; i++)
                results[i] = await adam.Create<T>(position(i), rotation(i));
            return results;
        }

        public static async UniTask<T[]> Create<T>(this Adam adam, int count, Func<int, IVector> position,
            Func<int, IVector> rotation) where T : class
        {
            var results = new T[count];
            for (var i = 0; i < count; i++)
                results[i] = await adam.Create<T>(position(i), rotation(i));
            return results;
        }

        // ── Destroy ───────────────────────────────────────────────────────────────

        public static async UniTask Destroy(this Adam adam, IEnumerable<object> objects)
        {
            foreach (var obj in objects)
                await adam.Destroy(obj);
        }

        public static async UniTask Destroy<T>(this Adam adam, IEnumerable<T> objects) where T : class
        {
            foreach (var obj in objects)
                await adam.Destroy(obj);
        }

        public static async UniTask Destroy(this Adam adam, params object[] objects)
        {
            foreach (var obj in objects)
                await adam.Destroy(obj);
        }
    }
}