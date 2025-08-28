using System;
using System.Collections.Generic;
using System.ComponentModel;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.GlobalState;
using UnityEngine;
using Zenject;
using ITickable = Zenject.ITickable;

namespace DAFP.TOOLS.Injection
{
    public abstract class GlobalInstaller<T, TCursorService> : Zenject.MonoInstaller where T : IGlobalGameStateHandler
        where TCursorService : IGlobalCursorStateHandler

    {
        protected abstract Dictionary<Type, Type> GetServices();
        protected abstract string GetGameDefaultGameState(InjectContext arg);
        protected abstract TCursorService GetCursorStateHandler(InjectContext arg);

        protected virtual void InstallTickers()
        {
            Container.Bind<ITickerBase>().WithId("DefaultPhysicsComponentGameplayTicker")
                .FromMethod((context => new FixedUpdateTicker<IEntityComponent>(new()))).AsSingle().Lazy();
            Container.Bind<ITicker<IEntity>>().WithId("DefaultUpdateEntityGameplayTicker")
                .FromMethod((context => new UpdateTicker<IEntity>(new()))).AsSingle().Lazy();
        }

        public override void InstallBindings()
        {
            Container.Bind<string>().WithId("DefaultGameState").FromMethod(GetGameDefaultGameState).AsCached();
            Container.Bind<TCursorService>()
                .FromMethod(GetCursorStateHandler)
                .AsSingle().NonLazy();
            // 2) Expose it via interface
            Container.Bind<IGlobalCursorStateHandler>()
                .To<TCursorService>()
                .FromResolve();

            // 3) Bind concrete game-state handler
            Container.Bind<T>()
                .AsSingle().NonLazy();
            // 4) Expose it via interface
            Container.Bind<IGlobalGameStateHandler>()
                .To<T>()
                .FromResolve();

            // Now these FromResolve() calls will succeed
            Container.Bind<ITickable>().To<TCursorService>().FromResolve();
            Container.Bind<IInitializable>().To<TCursorService>().FromResolve();
            Container.Bind<ITickable>().To<T>().FromResolve();
            Container.Bind<IInitializable>().To<T>().FromResolve();
            InstallTickers();
            if (GetServices().Count > 0)
            {
                foreach (var _singleManager in GetServices())
                {
                    Container.Bind(_singleManager.Key).To(_singleManager.Value).FromNewComponentOnNewGameObject()
                        .AsSingle().NonLazy();
                }
            }
        }
    }
}