using System;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using FluentResults;
using MessagePipe;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    //-- in charge of handling the creation and the destruction of all objects
    [Preserve]
    public sealed class Adam : IAsyncFactory<Adam.CreationInfo, object>, IAsyncDestroyer<object>
    {
        // [Inject] private World world;

        [Inject] private IAssetManager assetManager;
        [Inject] private IObjectCreatePreparer create_preparer;
        [Inject] private IObjectDestroyPreparer destroy_preparer;

        private ICreator[] creators;

        private IDestroyer[] destroyers;

        [Inject]
        private Adam(ICreator[] c, IDestroyer[] d)
        {
            creators = c.OrderByDescending((creator => creator.Priority)).ToArray();
            destroyers = d.OrderByDescending((destroyer => destroyer.Priority)).ToArray();
        }


        public async UniTask<object> Create(CreationInfo info)
        {
            foreach (var _creator in creators)
            {
                var _res = await _creator.Create(info);
                if (!_res.IsSuccess) continue;
                var _final = await create_preparer.Create(info, _res.Value);
                return _final;
            }

            throw new Exception($"[Adam] :: Tried to create an object of {info} but failed.");
        }

        public async UniTask Destroy(object obj)
        {
            if (obj == null)
                return;

            var _final = await destroy_preparer.Create(obj);

            foreach (var _destroyer in destroyers)
            {
                var _res = await _destroyer.Destroy(_final);
                if (_res.Result.IsSuccess)
                    return;
            }

            throw new Exception($"[Adam] :: Tried to destroy an object of {obj} but failed.");
        }

        public interface IObjectAssetCreationInfo
        {
        }

        [System.Serializable]
        public struct CreationInfo
        {
            private CreationInfo(IObjectAssetCreationInfo info, bool newObject, string objname, Type wantedType,
                INameable author = null)
            {
                asset_info = info;
                this.newObject = newObject;
                ObjName = objname;
                WantedType = wantedType;
                Author = author;
                requestedAtTick = Time.frameCount;

#if UNITY_EDITOR
                RequestedBy = Author?.Name;
#endif
            }

            internal IObjectAssetCreationInfo asset_info;
            internal bool newObject;
            internal string ObjName { get; }
            internal Type WantedType { get; set; }


            public readonly int RequestedAtTick => requestedAtTick;

            [ReadOnly][SerializeField] private int requestedAtTick;
            public INameable Author { get; }
#if UNITY_EDITOR

            [ReadOnly][SerializeField] private string RequestedBy;
#endif

            public CreationInfo WithType(Type T) => new(asset_info, newObject, ObjName, T);

            public static CreationInfo New<T>(string name, IObjectAssetCreationInfo info = default,
                INameable author = null) =>
                new(info, true, name, typeof(T), author);

            public static CreationInfo Scene() => Asset<Entity>(default, INameable.Literal("Scene: "+SceneManager.GetActiveScene().name));

            public static CreationInfo New<T>(IObjectAssetCreationInfo info = default, INameable author = null) =>
                New<T>(null, info, author);

            public static CreationInfo Asset<T>(GameAssetInfo info, INameable author = null) =>
                new(info, false, null, typeof(T), author);
        }

        [System.Serializable]
        public struct DestructionInfo
        {
            private DestructionInfo(CreationInfo info, Result result)
            {
                this.info = info;
                Result = result;
                CreatedAtTick = Time.frameCount;
            }

            internal readonly CreationInfo info;
            public readonly Result Result;
            public int CreatedAtTick { get; }
            public static DestructionInfo Success(CreationInfo info) => new(info, Result.Ok());

            public static DestructionInfo Failure() =>
                new(default, FluentResults.Result.Fail(new Error("Destruction Failed")));
        }
    }

    public interface ICreator : IAsyncFactory<Adam.CreationInfo, Result<object>>, IPrioritized
    {
    }


    public interface IDestroyer : IAsyncDestroyer<Adam.DestructionInfo, object>, IPrioritized
    {
    }
}