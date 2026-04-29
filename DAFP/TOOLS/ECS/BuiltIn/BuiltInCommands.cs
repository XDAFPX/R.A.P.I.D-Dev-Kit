using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using ModestTree;
using NRandom;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public static class BuiltInCommands
    {
        public abstract class ConsoleCommand : IConsoleCommand
        {
            public List<ICommandInterpreter> Children { get; } = new();
            public List<ICommandInterpreter> Owners { get; } = new();
            public abstract string Name { get; set; }
            public abstract IMessage Description { get; set; }

            public string Procces(string input)
            {
                if (!CommandParserUtils.CheckIfInputContainsCommand(input, Name)) return null;
                return Execute(input);
            }

            protected abstract string Execute(string input);
        }

        
        
        public class ClearCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "cls";
            public override IMessage Description { get; set; } = IMessage.Literal("Clears console");

            protected override string Execute(string input)
            {
                var _interpreter = CommandParserUtils.GetRoot(this);
                if (_interpreter is not IConsoleMessenger _console) return CommandParserUtils.GenericException();
                _console.Clear();
                return "pass";
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
                IMessage.Literal("displays the players in the current game");

            protected override string Execute(string input)
            {
                if (world.Players.IsEmpty())
                    return CommandParserUtils.NoPlayersException();

                return $"There are {world.Players.Count()} players ::\n " + string.Join("\n", world.Players
                    .Select(p => $" Player: name:'{p.Name}', local:{p.IsLocal}"));
            }
        }

        public class NoclipCommand : ConsoleCommand
        {
            private readonly World world;

            [Inject]
            public NoclipCommand(World world)
            {
                this.world = world;
            }

            public override string Name { get; set; } = "noclip";

            public override IMessage Description { get; set; } =
                CompText.Literal("disables the effect of gravity and makes the player nonsolid");

            protected override string Execute(string input)
            {
                var arg = CommandParserUtils.GetSingleCommandArgument(input, Name);
                var _pl = CommandParserUtils.ParsePlayer(arg, world.Players, out var _error);
                if (_pl == null) return _error;
                _pl.ToggleNoclip();
                return $"Toggled noclip on player {_pl.Name}";
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

            protected override string Execute(string input)
            {
                foreach (var _ownable in Children)
                {
                    if (_ownable is not IConsoleCommand cmd) continue;
                    if (!CommandParserUtils.CheckIfInputContainsCommand(input, cmd.Name)) continue;
                    return cmd.Procces(input.Replace(cmd.Name, ""));
                }

                return "use case: use a sub command with '_' prefix";
            }

            public class MatDebugViewCommand : ConsoleCommand
            {
                public override string Name { get; set; } = "mat_showdebug";

                public override IMessage Description { get; set; } =
                    new CompText(new Span("Sets the debug draw layer's value").I_RT());

                protected override string Execute(string input)
                {
                    var args = CommandParserUtils.ParseArguments(input);
                    var sys = CommandParserUtils.TryGetSys(this);
                    if (!args.IsInBounds(0) || string.IsNullOrEmpty(args[0]))
                        return CommandParserUtils.InvalidArgumentsException(this);
                    if (!args.IsInBounds(1) || string.IsNullOrEmpty(args[1]))
                        return CommandParserUtils.InvalidArgumentsException(this);
                    var layer = sys.Layers.FindByName(args[0]);
                    if (layer == default) return $"layer {args[0]} was not found";
                    if (!CommandParserUtils.TryGetBool(args[1], out var val))
                        return $"layer value({args[1]}) was invalid";
                    (layer as DebugDrawLayer).Enabled = val;
                    return "debug draw updated";
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

            protected override string Execute(string input)
            {
                var result = CommandParserUtils.GetSingleCommandArgument(input, Name);
                if (result == null) return CommandParserUtils.InvalidArgumentsException(this);
                if (int.TryParse(result, out var index))
                {
                    DefaultLevelTransition.Transition(index, saveSystem, world, random);
                    return $"Loading... {result}";
                }

                DefaultLevelTransition.Transition(result, saveSystem, world, random);
                return $"Loading... {result}";
            }
        }

        public class ChangeSceneCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "scene";
            public override IMessage Description { get; set; } = IMessage.Literal("Changes the scene");

            protected override string Execute(string input)
            {
                var result = CommandParserUtils.GetSingleCommandArgument(input, Name);
                if (result == null) return CommandParserUtils.InvalidArgumentsException(this);
                if (int.TryParse(result, out var a)) SceneManager.LoadSceneAsync(a);
                else SceneManager.LoadSceneAsync(result);
                return $"Changing scene...";
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

            protected override string Execute(string input)
            {
                var result = CommandParserUtils.GetSingleCommandArgument(input, Name);
                if (result == null) return CommandParserUtils.InvalidArgumentsException(this);
                sys.PlayOneShot(sys.GetDefault(), result);
                return $"playing {result} ...";
            }
        }

        public class VersionCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "ver";

            public override IMessage Description { get; set; } =
                IMessage.Literal("Displays the version of the game or unity (--unity)");

            protected override string Execute(string input)
            {
                if (CommandParserUtils.GetSingleCommandArgument(input, Name) == "--unity")
                    return $"unity's version: {Application.unityVersion}";
                return $"version: {Application.version}";
            }
        }

        public class QuitCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "q";
            public override IMessage Description { get; set; } = IMessage.Literal("Exits from the game");

            protected override string Execute(string input)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
                return "Quiting now...";
            }
        }

        public class HelpCommand : ConsoleCommand
        {
            public override string Name { get; set; } = "help";
            public override IMessage Description { get; set; } = IMessage.Literal("Displays all commands");

            protected override string Execute(string input)
            {
                var cmds = new List<IConsoleCommand>();
                GetPets(CommandParserUtils.GetRoot(this), cmds);
                return string.Join("\n", cmds.Select(cmd => $"{cmd.Name} : ({cmd.Description.Print()})"));
            }

            private void GetPets(ICommandInterpreter interpreter, List<IConsoleCommand> commands)
            {
                if (interpreter == default) return;
                if (interpreter is IConsoleCommand cmd) commands.Add(cmd);
                foreach (var child in interpreter.Pets)
                    if (child is ICommandInterpreter inter)
                        GetPets(inter, commands);
            }
        }
    }
}