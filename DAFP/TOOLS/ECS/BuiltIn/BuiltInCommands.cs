using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using ModestTree;
using NRandom;
using PixelRouge.Inspector.Extensions;
using R3;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public static class BuiltInCommands
    {
        public class ClearCommand : ConsoleCommand //--fixed 
        {
            public override string Name { get; set; } = "cls";
            public override IMessage Description { get; set; } = IMessage.Literal("Clears console");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _interpreter = CommandParserUtils.GetRoot(this);
                if (_interpreter is not IConsoleMessenger _console)
                {
                    context.Log.OnNext(CommandParserUtils.GenericException());
                    return UniTask.CompletedTask;
                }

                _console.Clear();

                return UniTask.CompletedTask;
            }
        }

        public class PlayersCommand : ConsoleCommand
        {
            private readonly World world;

            [Inject]
            public PlayersCommand(World world)
            {
                this.world = world;
            }

            public override string Name { get; set; } = "players";

            public override IMessage Description { get; set; } =
                IMessage.Literal("Displays the players in the current game.");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                if (world.Players.IsEmpty())
                {
                    context.Log.OnNext(CommandParserUtils.NoPlayersException());
                    return UniTask.CompletedTask;
                }

                context.Log.OnNext(IMessage.Literal($"There are {world.Players.Count()} players ::\n" + string.Join(
                    "\n", world.Players
                        .Select(p => $" Player: name:'{p.Body.Name}', local:{p.Data.IsLocal}"))));
                return UniTask.CompletedTask;
            }
        }

        public abstract class OnePlayerCommand : ConsoleCommand
        {
            protected readonly World World;

            [Inject]
            protected OnePlayerCommand(World world)
            {
                this.World = world;
            }

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _arg = CommandParserUtils.GetSingleCommandArgument(SourceInput, Name);
                var _pl = CommandParserUtils.ParsePlayer(_arg, World.Players, out var _error);
                if (_pl == null)
                {
                    context.Log.OnNext(_error);
                    return UniTask.CompletedTask;
                }

                return Execute(context, ct, _pl);
            }

            protected abstract UniTask Execute(TextProcessContext context, CancellationToken ct, IPlayer player);
        }

        public class Buddha : OnePlayerCommand, IHiddenCommand
        {
            [Inject]
            public Buddha(World world) : base(world)
            {
            }

            public override string Name { get; set; } = "buddha";

            public override IMessage Description { get; set; } =
                CompText.Literal("Makes the player invincible");

            protected override UniTask Execute(TextProcessContext context, CancellationToken ct, IPlayer player)
            {
                GameUtils.Buddha(player.Body);
                string _active = player.Body.Memory.Has("Buddha") ? "on" : "off";
                context.Log.OnNext(IMessage.Literal($"Buddha mode {_active}"));
                return UniTask.CompletedTask;
            }
        }

        public class GodCommand : OnePlayerCommand
        {
            [Inject]
            public GodCommand(World world) : base(world)
            {
            }

            public override string Name { get; set; } = "god";

            public override IMessage Description { get; set; } =
                CompText.Literal("Makes the player invincible");

            protected override UniTask Execute(TextProcessContext context, CancellationToken ct, IPlayer player)
            {
                GameUtils.God(player.Body);
                string _active = player.Body.Memory.Has("God") ? "ON" : "OFF";
                context.Log.OnNext(IMessage.Literal($"godmode {_active}"));
                return UniTask.CompletedTask;
            }
        }

        public class NoclipCommand : OnePlayerCommand
        {
            [Inject]
            public NoclipCommand(World world) : base(world)
            {
            }

            public override string Name { get; set; } = "noclip";

            public override IMessage Description { get; set; } =
                CompText.Literal("Disables the effect of gravity and makes the player nonsolid");

            protected override UniTask Execute(TextProcessContext context, CancellationToken ct, IPlayer player)
            {
                GameUtils.Noclip(player.Body);
                string _active = player.Body.Memory.Has("Noclip") ? "Active" : "Disabled";
                context.Log.OnNext(IMessage.Literal($"Noclip is [{_active}] on {player.Name}"));
                return UniTask.CompletedTask;
            }
        }

        public class MatCommand : ConsoleCommand
        {
            public MatCommand()
            {
                Children.Add(new MatDebugViewCommand());
                foreach (var _ownable in Children) _ownable.ChangeOwner(this);
            }

            public override string Name { get; set; } = "mat";
            public override IMessage Description { get; set; } = IMessage.Literal("Changes the view of entities");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                foreach (var _ownable in Children)
                {
                    if (_ownable is not IConsoleCommand _cmd) continue;
                    if (!CommandParserUtils.CheckIfInputContainsCommand(SourceInput, _cmd.Name)) continue;
                    return _cmd.Process(SourceInput.Replace(_cmd.Name, "")).Execute(context, ct);
                }

                context.Log.OnNext(IMessage.Literal("use case: use a sub command with '_' prefix"));
                return UniTask.CompletedTask;
            }

            public class MatDebugViewCommand : ConsoleCommand
            {
                public override string Name { get; set; } = "mat_showdebug";

                public override IMessage Description { get; set; } =
                    new CompText(new Span("Sets the debug draw layer's value").I_RT());

                public override UniTask Execute(TextProcessContext context, CancellationToken ct)
                {
                    var _args = CommandParserUtils.ParseArguments(SourceInput);
                    var _sys = CommandParserUtils.TryGetSys(this);
                    if (!_args.IsInBounds(0) || string.IsNullOrEmpty(_args[0]))
                    {
                        context.Log.OnNext(CommandParserUtils.InvalidArgumentsException(this));
                        return UniTask.CompletedTask;
                    }

                    if (!_args.IsInBounds(1) || string.IsNullOrEmpty(_args[1]))
                    {
                        context.Log.OnNext(CommandParserUtils.InvalidArgumentsException(this));
                        return UniTask.CompletedTask;
                    }

                    var _layer = _sys.Layers.FindByName(_args[0]);
                    if (_layer == default)
                    {
                        context.Log.OnNext(IMessage.Literal($"layer {_args[0]} was not found"));
                        return UniTask.CompletedTask;
                    }

                    if (!CommandParserUtils.TryGetBool(_args[1], out var _val))
                    {
                        context.Log.OnNext(IMessage.Literal($"layer value({_args[1]}) was invalid"));
                        return UniTask.CompletedTask;
                    }

                    (_layer as DebugDrawLayer).Enabled = _val;
                    context.Log.OnNext(IMessage.Literal("debug draw updated"));
                    return UniTask.CompletedTask;
                }
            }
        }

        public class LoadLevelCommand : ConsoleCommand
        {
            private readonly ISaveSystem saveSystem;
            private readonly IRandom random;
            private readonly World world;

            [Inject]
            public LoadLevelCommand(ISaveSystem saveSystem, IRandom random, World world)
            {
                this.saveSystem = saveSystem;
                this.random = random;
                this.world = world;
            }

            public override string Name { get; set; } = "map";
            public override IMessage Description { get; set; } = IMessage.Literal("Changes the map");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _result = CommandParserUtils.GetSingleCommandArgument(SourceInput, Name);
                if (_result == null)
                {
                    context.Log.OnNext(CommandParserUtils.InvalidArgumentsException(this));
                    return UniTask.CompletedTask;
                }

                if (int.TryParse(_result, out var _index))
                    DefaultLevelTransition.Transition(_index, saveSystem, world, random);
                else
                    DefaultLevelTransition.Transition(_result, saveSystem, world, random);
                context.Log.OnNext(IMessage.Literal($"Loading... {_result}"));
                return UniTask.CompletedTask;
            }
        }

        public class ChangeSceneCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "scene";
            public override IMessage Description { get; set; } = IMessage.Literal("Changes the scene");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _result = CommandParserUtils.GetSingleCommandArgument(SourceInput, Name);
                if (_result == null)
                {
                    context.Log.OnNext(CommandParserUtils.InvalidArgumentsException(this));
                    return UniTask.CompletedTask;
                }

                if (int.TryParse(_result, out var _a)) SceneManager.LoadSceneAsync(_a);
                else SceneManager.LoadSceneAsync(_result);
                context.Log.OnNext(IMessage.Literal("Changing scene..."));
                return UniTask.CompletedTask;
            }
        }

        public class PlayAudioCommand : ConsoleCommand
        {
            private readonly IAudioSystem sys;

            [Inject]
            public PlayAudioCommand(IAudioSystem sys)
            {
                this.sys = sys;
            }

            public override string Name { get; set; } = "play";
            public override IMessage Description { get; set; } = IMessage.Literal("plays a sound effect or music");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _result = CommandParserUtils.GetSingleCommandArgument(SourceInput, Name);
                if (_result == null)
                {
                    context.Log.OnNext(CommandParserUtils.InvalidArgumentsException(this));
                    return UniTask.CompletedTask;
                }

                sys.PlayOneShot(sys.GetDefault(), _result);
                context.Log.OnNext(IMessage.Literal($"playing {_result} ..."));
                return UniTask.CompletedTask;
            }
        }

        public class VersionCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "ver";

            public override IMessage Description { get; set; } =
                IMessage.Literal("Displays the version of the game or unity (--unity)");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                if (CommandParserUtils.GetSingleCommandArgument(SourceInput, Name) == "--unity")
                    context.Log.OnNext(IMessage.Literal($"unity's version: {Application.unityVersion}"));
                else
                    context.Log.OnNext(IMessage.Literal($"version: {Application.version}"));
                return UniTask.CompletedTask;
            }
        }

        public class QuitCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "q";
            public override IMessage Description { get; set; } = IMessage.Literal("Exits from the game");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                context.Log.OnNext(IMessage.Literal("Quiting now..."));
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
                return UniTask.CompletedTask;
            }
        }


        public class ShowFPSCommand : ConsoleCommand, IHiddenCommand
        {
            private readonly World world;
            private readonly InfoSystem infoSystem;
            public override string Name { get; set; } = "cl_show_fps";
            public override IMessage Description { get; set; } = IMessage.Literal("Shows FPS");

            [Inject]
            public ShowFPSCommand(World world, InfoSystem infoSystem)
            {
                this.world = world;
                this.infoSystem = infoSystem;
            }

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _sys = CommandParserUtils.TryGetSys(this);
                var _arg = CommandParserUtils.GetSingleCommandArgument(SourceInput, Name);
                if (CommandParserUtils.TryGetBool(_arg, out var _value))
                {
                    if (_value)
                    {
                        _sys.AddDebugValue(
                            new DebugValue()
                            {
                                Name = "fps",
                                Stream = (() => infoSystem.CurrentFPS.ToString(CultureInfo.InvariantCulture))
                            });
                    }
                    else
                    {
                        _sys.RemoveDebugValue("fps");
                    }
                }
                else
                {
                    context.Log.OnNext(IMessage.Literal("error happened"));
                }

                return UniTask.CompletedTask;
            }
        }

        public class ShowPosCommand : ConsoleCommand,IHiddenCommand
        {
            private readonly World world;
            public override string Name { get; set; } = "cl_show_pos";
            public override IMessage Description { get; set; } = IMessage.Literal("Shows position and velocity ");

            [Inject]
            public ShowPosCommand(World world)
            {
                this.world = world;
            }

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _sys = CommandParserUtils.TryGetSys(this);
                var _arg = CommandParserUtils.GetSingleCommandArgument(SourceInput, Name);
                var _pl = CommandParserUtils.ParsePlayer("", world.Players, out var _error);
                if (CommandParserUtils.TryGetBool(_arg, out var _value) && _pl != null)
                {
                    if (_value)
                    {
                        _sys.AddDebugValue(
                            new DebugValue() { Name = "pos", Stream = (() => _pl.Body.Pos().ToString()) });
                        _sys.AddDebugValue(
                            new DebugValue()
                            {
                                Name = "vel",
                                Stream = (() =>
                                    _pl.Body.Stats.Get("Velocity", () => new QuikStat<Vector3>()).Value.ToString())
                            });
                        _sys.AddDebugValue(
                            new DebugValue()
                            {
                                Name = "ang", Stream = (() => _pl.Body.EyeVector.ToString())
                            });
                    }
                    else
                    {
                        _sys.RemoveDebugValue("pos");
                        _sys.RemoveDebugValue("vel");
                        _sys.RemoveDebugValue("ang");
                    }
                }
                else
                {
                    context.Log.OnNext(IMessage.Literal("error happened"));
                }

                return UniTask.CompletedTask;
            }
        }


        public class HelpCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "help";
            public override IMessage Description { get; set; } = IMessage.Literal("Displays all commands");

            public override UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                var _cmds = new List<IConsoleCommand>();
                GetPets(CommandParserUtils.GetRoot(this), _cmds);
                context.Log.OnNext(IMessage.Literal(
                    string.Join("\n \n", _cmds.Select(cmd => $"  {cmd.Name} : ({cmd.Description.Print()})")) + "\n "));
                return UniTask.CompletedTask;
            }

            protected virtual void GetPets(ICommandInterpreter interpreter, List<IConsoleCommand> commands)
            {
                if (interpreter == default) return;
                if (interpreter is IConsoleCommand _cmd && Filter(_cmd)) commands.Add(_cmd);
                foreach (var _child in interpreter.Pets)
                    if (_child is ICommandInterpreter _inter)
                        GetPets(_inter, commands);
            }

            protected virtual bool Filter(IConsoleCommand cmd)
            {
                return cmd is not IHiddenCommand;
            }
        }
    }
}