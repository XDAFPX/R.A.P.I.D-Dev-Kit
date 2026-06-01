using System;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.GlobalState.CursorSates;
using DAFP.TOOLS.ECS.GlobalState.GameStates;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using DAFP.TOOLS.ECS.UI;
using NRandom;
using TNRD;
using TripleA.Utils.Extensions;
using UGizmo;
using UnityEngine;
using UnityEventBus;
using Zenject;
using ITickable = Zenject.ITickable;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.Injection
{
    public abstract class
        VideoGame<TWorld, TGameStateService, TCursorService, TSaveService, TSettingsSaveService,
            TAudioService, TRandomService, TConsoleService, TGizmosService, TDebugService,
            TCommandInterpreter, TModManager> : MonoInstaller, IVideoGame
        where TWorld : World
        where TGameStateService : IGameStateHandler
        where TCursorService : ICursorStateHandler
        where TSaveService : ISaveSystem
        where TSettingsSaveService : IGlobalSettingsSaveSystem
        where TAudioService : IAudioSystem
        where TRandomService : IRandom, new()
        where TConsoleService : IConsoleMessenger
        where TGizmosService : IGlobalGizmos
        where TDebugService : IDebugSys<TGizmosService, TConsoleService>, IDebugSys<IGlobalGizmos, IConsoleMessenger>
        where TCommandInterpreter : ICommandInterpreter
        where TModManager : IModManager

    {
        [SerializeField] private GameObject[] UISystemPrefabs;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private SerializableInterface<IMod>[] Mods;

        protected virtual bool GetDefaultConsoleUnlockState(InjectContext arg) => Application.isEditor;


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
                typeof(BuiltInCommands.GodCommand),
                typeof(BuiltInCommands.Buddha),
                typeof(BuiltInCommands.ShowFPSCommand),
                typeof(BuiltInCommands.ShowPosCommand)
            };
        }

        private void bind_commands()
        {
            foreach (var _consoleCommand in GetDefaultConsoleCommands())
                Container.Bind<IConsoleCommand>().To(_consoleCommand).AsTransient().Lazy();
        }

        protected virtual void InstallTickers()
        {
            Container.Bind<ITicker>().WithId(IVideoGame.PHYSICS_UPDATE)
                .FromMethod(_ => new FixedUpdateTicker(new HashSet<IGameState>())).AsTransient().Lazy();

            Container.Bind<ITicker>().WithId(IVideoGame.VIEW_MODEL_UPDATE)
                .FromMethod(_ => new UpdateTicker(new(), 10)).AsTransient().Lazy();

            Container.Bind<ITicker>().WithId(IVideoGame.DEFAULT_UPDATE)
                .FromMethod(_ => new UpdateTicker(new HashSet<IGameState>())).AsTransient().Lazy();

            Container.Bind<ITicker>().WithId(IVideoGame.EFFECTS_UPDATE)
                .FromMethod(_ => new FixedUpdateTicker(new HashSet<IGameState>())).AsTransient().Lazy();
        }

        protected abstract void InstallAdditional();

        public sealed override void InstallBindings()
        {
            bind_constants();
            bind_info_system();
            bind_audio();
            bind_asset_manager();
            bind_input_manager();
            bind_debug_systems();
            bind_random_systems();
            bind_cursor_systems();
            bind_game_state_systems();
            bind_global_boards();
            InstallTickers();
            bind_world();
            bind_save_systems();
            bind_ui_systems();
            bind_console();
            BindCameraManager();
            bind_mod_manager();
            bind_compatability_sys();

            InstallAdditional();
        }

        private void bind_compatability_sys()
        {
            BindCompatabilitySys();
            Container.BindInterfacesAndSelfTo<CompatabilityChecker>().AsCached();
        }

        protected virtual void BindCompatabilitySys()
        {
            Container.BindInterfacesAndSelfTo<CompatabilitySys>().AsSingle().Lazy();
        }

        protected virtual void BindCameraManager()
        {
            Container.Bind<ICameraManager>().To<UniversalCameraManager>().AsSingle();
            Container.Bind<IInitializable>().To<ICameraManager>().FromResolve();
            Container.Bind<ITickable>().To<ICameraManager>().FromResolve();
        }

        protected virtual void bind_constants()
        {
            // Container.Bind<>().WithId("DefaultGameState").FromMethod(GetGameDefaultGameState).AsCached();
            // Container.Bind<string>().WithId("DefaultDifficulty").FromMethod(GetDefaultDifficultyState).AsCached();
            // Container.Bind<string>().WithId("DefaultDifficultyDomain").FromMethod(GetDefaultDifficultyStateDomainName)
            //     .AsCached();
            // Container.Bind<string>().WithId("DefaultCursorState").FromMethod(GetDefaultCursorState).AsCached();
            Container.Bind<bool>().WithId("ConsoleUnlocked").FromMethod(GetDefaultConsoleUnlockState).AsCached();
            Container.Bind<string>().WithId("SystemCompatible").FromMethod((context => IsSystemCompatible)).AsCached();

            Container.Bind<IEventBus>().WithId(IVideoGame.GAME_BUS_NAME)
                .FromMethod(_ => new GlobalStateBus()).AsCached().NonLazy();
        }


        protected virtual string IsSystemCompatible => null;

        private void bind_info_system()
        {
            Container.Bind<InfoSystem>().AsSingle();
            Container.Bind<ITickable>().To<InfoSystem>().FromResolve();
        }

        private void bind_input_manager()
        {
            Container.Bind<ControllerManager>().AsSingle().NonLazy();
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
            Container.Bind<IGlobalCursorState>().WithId("DefaultCursorState").FromMethod(GetDefaultCursor);
            Container.Bind<TCursorService>().AsSingle().NonLazy();
            Container.Bind<ICursorStateHandler>().To<TCursorService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TCursorService>().FromResolve();
            Container.Bind<ITickable>().To<TCursorService>().FromResolve();
            Container.Bind<IInitializable>().To<TCursorService>().FromResolve();
        }

        protected virtual IGlobalCursorState GetDefaultCursor(InjectContext context)
        {
            return new BasicCursorState(null, new CursorSettings() { IsVisible = true, Mode = CursorLockMode.None },
                "Default");
        }

        private void bind_game_state_systems()
        {
            Container.Bind<IGameState>().WithId("DefaultGameState").To(GetDefaultGameState()).AsSingle().NonLazy();
            Container.Bind<TGameStateService>().AsSingle().NonLazy();
            Container.Bind<IGameStateHandler>().To<TGameStateService>().FromResolve();
            Container.Bind<IGlobalStateHandlerBase>().To<TGameStateService>().FromResolve();
            Container.Bind<ITickable>().To<TGameStateService>().FromResolve();
            Container.Bind<IInitializable>().To<TGameStateService>().FromResolve();
        }


        protected virtual Type GetDefaultGameState()
        {
            return typeof(NormalGameState<NormalCursorState>);
        }

        private void bind_audio()
        {
            Container.Bind<TAudioService>().AsSingle().NonLazy();
            Container.Bind<IAudioSystem>().To<TAudioService>().FromResolve();

            Container.BindInterfacesAndSelfTo<MusicMan>().AsSingle()
                .WithArguments("Main").Lazy();
        }


        private void bind_global_boards()
        {
            Container.Bind<UtillGlobalBoard>().AsSingle().NonLazy();
            Container.Bind<GlobalBlackBoard>().To<UtillGlobalBoard>().FromResolve();
            Container.Bind<ITickable>().To<UtillGlobalBoard>().FromResolve();
        }

        private void bind_world()
        {
            Container.Bind<World>()
                .To<TWorld>()
                .AsSingle()
                .NonLazy();
            Container.Bind<IInitializable>().To<World>().FromResolve();
            Container.Bind<ITickable>().To<World>().FromResolve();
            Container.Bind<IFixedTickable>().To<World>().FromResolve();

            // Container.Bind<IInitializable>().To<TWorld>().FromResolve();
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

            Container.BindInterfacesAndSelfTo<TCommandInterpreter>().AsSingle().NonLazy();
            Container.Bind<ICommandInterpreter>()
                .WithId("ConsoleCommandInterpreter")
                .To<TCommandInterpreter>()
                .FromResolve();

            Container.Bind<TConsoleService>().AsSingle().NonLazy();
            Container.Bind<TGizmosService>().AsSingle().NonLazy();
            Container.Bind<TDebugService>().AsSingle().NonLazy();
            Container.Bind<ITickable>().To<TDebugService>().FromResolve();
            Container.Bind<IDebugSys<IGlobalGizmos, IConsoleMessenger>>().To<TDebugService>().FromResolve();
        }

        private void bind_asset_manager()
        {
            Container.BindInterfacesAndSelfTo<AssetFactory>().AsCached();
            BindAssetPools();
            Container.BindInterfacesAndSelfTo<UniversalAssetManager>().AsSingle().NonLazy();
        }

        protected virtual void BindAssetPools()
        {
            Container.Bind<IAssetPoolBase>().To<AssetPool<EntSpecialEffect,EntSpecialEffect>>().AsSingle()
                .WithArguments("Effects");
        }

        private void bind_mod_manager()
        {
            Container.Bind<TModManager>().AsSingle().NonLazy();
            Container.Bind<IMod[]>().FromMethod((context => Mods.ToValues().ToArray())).AsCached();
            Container.Bind<IModManager>().To<TModManager>().FromResolve();
        }
    }

    internal class CompatabilityChecker : IInitializable
    {
        [Inject] private CompatabilitySys sys;
        [Inject(Id = "SystemCompatible")] private string systemCompatible;

        public void Initialize()
        {
            if (systemCompatible.IsNullOrEmpty())
                return;
            sys.TriggerIncompatibility(systemCompatible);
        }
    }

    public interface IVideoGame
    {
        public const string GAME_BUS_NAME = "GameBus";
        public const string PHYSICS_UPDATE = "PhysicsUpdate";
        public const string DEFAULT_UPDATE = "DefaultUpdate";
        public const string VIEW_MODEL_UPDATE = "ViewModelUpdate";
        public const string EFFECTS_UPDATE = "EffectsUpdate";
    }
}