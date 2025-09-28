using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.UI;
using NRandom;
using NRandom.Unity;
using PixelRouge.Direction;
using UnityEngine;
using UnityEventBus;
using Zenject;
using ITickable = Zenject.ITickable;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.Injection
{
    public abstract class
        GlobalInstaller<TGameStateService, TCursorService, TSaveService, TConfigService, TSettingsSaveService,
            TAudioService, TRandomService> : Zenject.MonoInstaller
        where TGameStateService : IGlobalGameStateHandler
        where TCursorService : IGlobalCursorStateHandler
        where TSaveService : ISaveSystem
        where TConfigService : IGlobalConfigStateHandler
        where TSettingsSaveService : IGlobalSettingsSaveSystem
        where TAudioService : IAudioSystem
        where TRandomService : IRandom, new()
    {
        [SerializeField] private GameObject[] _uiSystemPrefabs;

        protected abstract Dictionary<Type, Type> GetServices();
        protected abstract string GetGameDefaultGameState(InjectContext arg);
        protected abstract string GetDefaultDifficultyState(InjectContext arg);
        protected abstract string GetDefaultCursorState(InjectContext arg);
        protected abstract string GetDefaultDifficultyStateDomainName(InjectContext arg);

        /// <summary>
        /// 2) Return the prefab list for binding
        /// </summary>
        protected virtual IEnumerable<GameObject> GetUISystemPrefabs()
        {
            return _uiSystemPrefabs ?? Array.Empty<GameObject>();
        }

        protected virtual void InstallTickers()
        {
            Container.Bind<ITickerBase>().WithId("DefaultPhysicsComponentGameplayTicker")
                .FromMethod(_ => new FixedUpdateTicker<IEntityComponent>(new())).AsSingle().Lazy();
            Container.Bind<ITicker<IEntity>>().WithId("DefaultUpdateEntityGameplayTicker")
                .FromMethod(_ => new UpdateTicker<IEntity>(new())).AsSingle().Lazy();
        }

        public override void InstallBindings()
        {
            Container.Bind<string>().WithId("DefaultGameState").FromMethod(GetGameDefaultGameState).AsCached();
            Container.Bind<string>().WithId("DefaultDifficulty").FromMethod(GetDefaultDifficultyState).AsCached();
            Container.Bind<string>().WithId("DefaultDifficultyDomain").FromMethod(GetDefaultDifficultyStateDomainName)
                .AsCached();
            Container.Bind<string>().WithId("DefaultCursorState").FromMethod(GetDefaultCursorState).AsCached();
            Container.Bind<IEventBus>().WithId("GlobalStateBus").FromMethod((context => new GlobalStateBus()))
                .AsSingle().NonLazy();


            Container.Bind<TRandomService>()
                .FromMethod((context =>
                {
                    var a = new TRandomService();
                    a.InitState((uint)Mathf.RoundToInt(777 + Random.value * 100));
                    return a;
                })).AsSingle().NonLazy();
            Container.Bind<IRandom>().To<TRandomService>().FromResolve();


            Container.Bind<TCursorService>().AsSingle().NonLazy();
            Container.Bind<IGlobalCursorStateHandler>().To<TCursorService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TCursorService>().FromResolve();

            Container.Bind<TGameStateService>().AsSingle().NonLazy();
            Container.Bind<IGlobalGameStateHandler>().To<TGameStateService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TGameStateService>().FromResolve();
            Container.Bind<ITickable>().To<TGameStateService>().FromResolve();
            Container.Bind<IInitializable>().To<TGameStateService>().FromResolve();

            Container.Bind<TAudioService>().AsSingle().NonLazy();
            Container.Bind<IAudioSystem>().To<TAudioService>().FromResolve();

            Container.Bind<TConfigService>().AsSingle().NonLazy();
            Container.Bind<IGlobalConfigStateHandler>().To<TConfigService>().FromResolve();
            Container.Bind<ITickable>().To<TConfigService>().FromResolve();
            Container.Bind<IInitializable>().To<TConfigService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TConfigService>().FromResolve();

            Container.Bind<ITickable>().To<TCursorService>().FromResolve();
            Container.Bind<IInitializable>().To<TCursorService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TCursorService>().FromResolve();

            Container.Bind<UtillGlobalBoard>().AsSingle().NonLazy();
            Container.Bind<GlobalBlackBoard>().To<UtillGlobalBoard>().FromResolve();
            Container.Bind<ITickable>().To<UtillGlobalBoard>().FromResolve();

            InstallTickers();

            if (GetServices().Count > 0)
            {
                foreach (var kvp in GetServices())
                {
                    Container.Bind(kvp.Key)
                        .To(kvp.Value)
                        .FromNewComponentOnNewGameObject()
                        .AsSingle()
                        .NonLazy();
                }
            }

            Container.Bind<TSaveService>().AsSingle().NonLazy();
            Container.Bind<ISaveSystem>().To<TSaveService>().FromResolve();
            Container.Bind<TSettingsSaveService>().AsSingle().NonLazy();
            Container.Bind<IGlobalSettingsSaveSystem>().To<TSettingsSaveService>().FromResolve();

            foreach (var prefab in GetUISystemPrefabs())
            {
                Container.Bind<IUISystem<IUIElement>>()
                    .FromComponentInNewPrefab(prefab)
                    .AsTransient()
                    .Lazy();
            }

            Container.BindInterfacesAndSelfTo<RootUISys>().AsSingle().NonLazy();
        }
    }
}