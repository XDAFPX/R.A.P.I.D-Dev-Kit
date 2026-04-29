using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.GAME.Assets;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using DAFP.TOOLS.ECS.UI;
using NRandom;
using NRandom.Unity;
using PixelRouge.Direction;
using TNRD;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using Zenject;
using ITickable = Zenject.ITickable;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.Injection
{
    public abstract class
        VideoGame<TGameStateService, TCursorService, TSaveService, TConfigService, TSettingsSaveService,
            TAudioService, TRandomService, TConsoleService, TGizmosService, TDebugService,
            TConsoleInterpriter, TAssetManager, TModManager> : MonoInstaller
        where TGameStateService : IGlobalGameStateHandler
        where TCursorService : IGlobalCursorStateHandler
        where TSaveService : ISaveSystem
        where TConfigService : IGlobalConfigStateHandler
        where TSettingsSaveService : IGlobalSettingsSaveSystem
        where TAudioService : IAudioSystem
        where TRandomService : IRandom, new()
        where TConsoleService : IConsoleMessenger
        where TGizmosService : IGlobalGizmos
        where TDebugService : IDebugSys<TGizmosService, TConsoleService>, IDebugSys<IGlobalGizmos, IConsoleMessenger>
        where TConsoleInterpriter : ICommandInterpreter
        where TAssetManager : IAssetManager
        where TModManager : IModManager

    {
        [SerializeField] private GameObject[] UISystemPrefabs;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private SerializableInterface<IMod>[] Mods;

        protected abstract Dictionary<Type, Type> GetServices();
        protected abstract string GetGameDefaultGameState(InjectContext arg);
        protected abstract string GetDefaultDifficultyState(InjectContext arg);
        protected abstract string GetDefaultCursorState(InjectContext arg);
        protected abstract string GetDefaultDifficultyStateDomainName(InjectContext arg);
        protected abstract bool GetDefaultConsoleUnlockState(InjectContext arg);

        protected virtual Color GetDefaultConsoleColor()
        {
            return Color.white;
        }

        protected abstract Font GetDefaultConsoleFont(InjectContext arg);


        protected IList<DebugDrawLayer> GetDefaultDebugDrawLayers(InjectContext arg)
        {
            return new List<DebugDrawLayer>
            {
                Shared,
                new(DebugDrawLayer.DefaultDebugLayers.TRIGGERS, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.HIT_BOXES, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.BOUNDING_BOXES, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.POSITIONS, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.EYE_VECTORS, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.VELOCITIES, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.NAMES, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.ENT_INFO, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.VIEW_MODELS, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.THINKERS, Application.isEditor),
                new(DebugDrawLayer.DefaultDebugLayers.MISC, Application.isEditor),
            };
        }

        private static readonly DebugDrawLayer Shared = new("Shared", true);

        protected DebugDrawLayer GetDefaultSharedDebugDrawLayer(InjectContext arg)
        {
            return Shared;
        }

        /// 2) Return the prefab list for binding
        /// </summary>
        protected virtual IEnumerable<GameObject> GetUISystemPrefabs()
        {
            return UISystemPrefabs ?? Array.Empty<GameObject>();
        }

        protected virtual float GetDefaultConsoleFontSize()
        {
            return 23;
        }

        protected virtual IEnumerable<Type> GetDefaultConsoleCommands()
        {
            return new Type[]
            {
                typeof(BuiltInCommands.HelpCommand),
                typeof(BuiltInCommands.QuitCommand),
                typeof(BuiltInCommands.LoadLevelCommand),
                typeof(BuiltInCommands.VersionCommand),
                typeof(BuiltInCommands.ChangeSceneCommand),
                typeof(BuiltInCommands.PlayAudioCommand),
                typeof(BuiltInCommands.MatCommand),
                typeof(BuiltInCommands.NoclipCommand),
                typeof(BuiltInCommands.PlayersCommand),
                typeof(BuiltInCommands.ClearCommand),
            };
        }

        private void bind_commands()
        {
            foreach (var _consoleCommand in GetDefaultConsoleCommands())
                Container.Bind<IConsoleCommand>().To(_consoleCommand).AsTransient().Lazy();
        }

        protected virtual void InstallTickers()
        {
            Container.Bind<ITickerBase>().WithId("DefaultPhysicsComponentGameplayTicker")
                .FromMethod(_ => new FixedUpdateTicker<IEntityComponent>(new HashSet<IGlobalGameState>())).AsTransient()
                .Lazy();

            Container.Bind<ITicker<IEntity>>().WithId("DefaultEffectsEntityGameplayTicker")
                .FromMethod(_ => new FixedUpdateTicker<IEntity>(new HashSet<IGlobalGameState>())).AsTransient().Lazy();
            Container.Bind<ITicker<IEntity>>().WithId("DefaultUpdateEntityGameplayTicker")
                .FromMethod(_ => new UpdateTicker<IEntity>(new HashSet<IGlobalGameState>())).AsTransient().Lazy();
        }

        protected abstract void InstallAdditional();

        public sealed override void InstallBindings()
        {
            bind_default_strings();
            bind_asset_manager();
            bind_input_manager();
            bind_debug_systems();
            bind_random_systems();
            bind_cursor_systems();
            bind_game_state_systems();
            bind_audio();
            bind_config();
            bind_global_boards();
            InstallTickers();
            bind_dynamic_services();
            bind_save_systems();
            bind_ui_systems();
            bind_console();
            bind_mod_manager();
            InstallAdditional();
        }


        private void bind_default_strings()
        {
            Container.Bind<string>().WithId("DefaultGameState").FromMethod(GetGameDefaultGameState).AsCached();
            Container.Bind<string>().WithId("DefaultDifficulty").FromMethod(GetDefaultDifficultyState).AsCached();
            Container.Bind<string>().WithId("DefaultDifficultyDomain").FromMethod(GetDefaultDifficultyStateDomainName)
                .AsCached();
            Container.Bind<string>().WithId("DefaultCursorState").FromMethod(GetDefaultCursorState).AsCached();
            Container.Bind<float>().WithId("ConsoleTextSize").FromMethod(GetDefaultConsoleFontSize).AsCached();
            Container.Bind<Font>().WithId("ConsoleFont").FromMethod(GetDefaultConsoleFont).AsCached();
            Container.Bind<bool>().WithId("ConsoleUnlocked").FromMethod(GetDefaultConsoleUnlockState).AsCached();
            Container.Bind<Color>().WithId("ConsoleColor").FromMethod(GetDefaultConsoleColor).AsCached();
            Container.Bind<IEventBus>().WithId("GlobalStateBus")
                .FromMethod(_ => new GlobalStateBus()).AsCached().NonLazy();

            Container.Bind<IEventBus>().WithId("GlobalGameEventsBus")
                .FromMethod(_ => new GlobalStateBus()).AsCached().NonLazy();
        }


        private void bind_input_manager()
        {
            Container.Bind<UniversalInputControllerManager>().AsSingle().NonLazy();
        }

        private void bind_debug_systems()
        {
            Container.Bind<IList<DebugDrawLayer>>()
                .FromMethod(GetDefaultDebugDrawLayers).AsSingle();

            Container.Bind<DebugDrawLayer>()
                .FromMethod(GetDefaultSharedDebugDrawLayer).AsSingle();
        }


        private void bind_random_systems()
        {
            Container.Bind<TRandomService>()
                .FromMethod(_ =>
                {
                    var _r = new TRandomService();
                    _r.InitState((uint)Mathf.RoundToInt(777 + Random.value * 100));
                    return _r;
                }).AsSingle().NonLazy();

            Container.Bind<IRandom>().To<TRandomService>().FromResolve();
        }

        private void bind_cursor_systems()
        {
            Container.Bind<TCursorService>().AsSingle().NonLazy();
            Container.Bind<IGlobalCursorStateHandler>().To<TCursorService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TCursorService>().FromResolve();
            Container.Bind<ITickable>().To<TCursorService>().FromResolve();
            Container.Bind<IInitializable>().To<TCursorService>().FromResolve();
        }

        private void bind_game_state_systems()
        {
            Container.Bind<TGameStateService>().AsSingle().NonLazy();
            Container.Bind<IGlobalGameStateHandler>().To<TGameStateService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TGameStateService>().FromResolve();
            Container.Bind<ITickable>().To<TGameStateService>().FromResolve();
            Container.Bind<IInitializable>().To<TGameStateService>().FromResolve();
        }

        private void bind_audio()
        {
            Container.Bind<TAudioService>().AsSingle().NonLazy();
            Container.Bind<IAudioSystem>().To<TAudioService>().FromResolve();
        }

        private void bind_config()
        {
            Container.Bind<TConfigService>().AsSingle().NonLazy();
            Container.Bind<IGlobalConfigStateHandler>().To<TConfigService>().FromResolve();
            Container.Bind<ITickable>().To<TConfigService>().FromResolve();
            Container.Bind<IInitializable>().To<TConfigService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TConfigService>().FromResolve();
        }

        private void bind_global_boards()
        {
            Container.Bind<UtillGlobalBoard>().AsSingle().NonLazy();
            Container.Bind<GlobalBlackBoard>().To<UtillGlobalBoard>().FromResolve();
            Container.Bind<ITickable>().To<UtillGlobalBoard>().FromResolve();
        }

        private void bind_dynamic_services()
        {
            if (GetServices().Count == 0) return;

            foreach (var _kv in GetServices())
                Container.Bind(_kv.Key)
                    .To(_kv.Value)
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();
        }

        private void bind_save_systems()
        {
            Container.Bind<TSaveService>().AsSingle().NonLazy();
            Container.Bind<ISaveSystem>().To<TSaveService>().FromResolve();

            Container.Bind<TSettingsSaveService>().AsSingle().NonLazy();
            Container.Bind<IGlobalSettingsSaveSystem>().To<TSettingsSaveService>().FromResolve();
        }

        private void bind_ui_systems()
        {
            foreach (var _prefab in GetUISystemPrefabs())
                Container.Bind<IUISystem<IUIElement>>()
                    .FromComponentInNewPrefab(_prefab)
                    .AsTransient()
                    .Lazy();

            Container.BindInterfacesAndSelfTo<RootUISys>().AsSingle().NonLazy();
        }

        private void bind_console()
        {
            bind_commands();

            Container.BindInterfacesAndSelfTo<TConsoleInterpriter>().AsSingle().NonLazy();
            Container.Bind<ICommandInterpreter>()
                .WithId("ConsoleCommandInterpriter")
                .To<TConsoleInterpriter>()
                .FromResolve();

            Container.Bind<TConsoleService>().AsSingle().NonLazy();
            Container.Bind<TGizmosService>().AsSingle().NonLazy();
            Container.Bind<TDebugService>().AsSingle().NonLazy();
            Container.Bind<ITickable>().To<TDebugService>().FromResolve();
            Container.Bind<IDebugSys<IGlobalGizmos, IConsoleMessenger>>().To<TDebugService>().FromResolve();
        }

        private void bind_asset_manager()
        {
            Container.Bind<IAssetFactory.DefaultAssetFactory>().AsCached();
            Container.Bind<TAssetManager>().AsSingle().NonLazy();
            Container.Bind<IAssetManager>().To<TAssetManager>().FromResolve();
            Container.Bind<IInitializable>().To<TAssetManager>().FromResolve();
        }

        private void bind_mod_manager()
        {
            Container.Bind<TModManager>().AsSingle().NonLazy();
            Container.Bind<IMod[]>().FromMethod((context => Mods.ToValues().ToArray())).AsCached();
            Container.Bind<IModManager>().To<TModManager>().FromResolve();
        }
    }
}