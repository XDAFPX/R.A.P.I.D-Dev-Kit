using System;
using System.Collections.Generic;
using System.ComponentModel;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using Zenject;
using ITickable = Zenject.ITickable;

namespace DAFP.TOOLS.Injection
{
    public abstract class
        GlobalInstaller<TGameStateService, TCursorService, TSaveService, TConfigService, TSettingsSaveService> : Zenject.MonoInstaller
        where TGameStateService : IGlobalGameStateHandler
        where TCursorService : IGlobalCursorStateHandler
        where TSaveService : ISaveSystem
        where TConfigService : IGlobalConfigStateHandler
        where TSettingsSaveService : IGlobalSettingsSaveSystem

    {
        protected abstract Dictionary<Type, Type> GetServices();
        protected abstract string GetGameDefaultGameState(InjectContext arg);
        protected abstract string GetDefaultDifficultyState(InjectContext arg);
        protected abstract string GetDefaultCursorState(InjectContext arg);
        protected abstract string GetDefaultDifficultyStateDomainName(InjectContext arg);

        protected virtual void InstallTickers()
        {
            Container.Bind<ITickerBase>().WithId("DefaultPhysicsComponentGameplayTicker")
                .FromMethod((context => new FixedUpdateTicker<IEntityComponent>(new()))).AsSingle().Lazy();
            Container.Bind<ITicker<IEntity>>().WithId("DefaultUpdateEntityGameplayTicker")
                .FromMethod((context => new UpdateTicker<IEntity>(new()))).AsSingle().Lazy();
        }

        public override void InstallBindings()
        {
            Container.Bind<GlobalStates>().AsSingle().NonLazy();
            Container.Bind<string>().WithId("DefaultGameState").FromMethod(GetGameDefaultGameState).AsCached();
            Container.Bind<string>().WithId("DefaultDifficulty").FromMethod(GetDefaultDifficultyState).AsCached();
            Container.Bind<string>().WithId("DefaultDifficultyDomain").FromMethod(GetDefaultDifficultyStateDomainName)
                .AsCached();
            Container.Bind<string>().WithId("DefaultCursorState").FromMethod((GetDefaultCursorState))
                .AsCached();
            Container.Bind<TCursorService>()
                .AsSingle().NonLazy();
            // 2) Expose it via interface
            Container.Bind<IGlobalCursorStateHandler>()
                .To<TCursorService>()
                .FromResolve();

            // 3) Bind concrete game-state handler
            Container.Bind<TGameStateService>()
                .AsSingle().NonLazy();
            // 4) Expose it via interface
            Container.Bind<IGlobalGameStateHandler>()
                .To<TGameStateService>()
                .FromResolve();

            Container.Bind<TConfigService>().AsSingle().NonLazy();
            Container.Bind<IGlobalConfigStateHandler>().To<TConfigService>().FromResolve();
            
            
            
            Container.Bind<ITickable>().To<TConfigService>().FromResolve();
            Container.Bind<IInitializable>().To<TConfigService>().FromResolve();
            Container.Bind<ITickable>().To<TCursorService>().FromResolve();
            Container.Bind<IInitializable>().To<TCursorService>().FromResolve();
            Container.Bind<ITickable>().To<TGameStateService>().FromResolve();
            Container.Bind<IInitializable>().To<TGameStateService>().FromResolve();
            
            
            InstallTickers();
            if (GetServices().Count > 0)
            {
                foreach (var _singleManager in GetServices())
                {
                    Container.Bind(_singleManager.Key).To(_singleManager.Value).FromNewComponentOnNewGameObject()
                        .AsSingle().NonLazy();
                }
            }

            Container.Bind<TSaveService>().AsSingle().NonLazy();
            Container.Bind<ISaveSystem>().To<TSaveService>().FromResolve();
            Container.Bind<TSettingsSaveService>().AsSingle().NonLazy();
            Container.Bind<IGlobalSettingsSaveSystem>().To<TSettingsSaveService>().FromResolve();
        }
    }
}