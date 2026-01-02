using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using ModestTree;
using NRandom;
using UGizmo;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCommandInterpriter : ICommandInterpriter
    {
        private ISet<IOwnable<ICommandInterpriter>> pets;


        public UniversalCommandInterpriter(IEnumerable<IConsoleCommand> commands)
        {
            pets = new HashSet<IOwnable<ICommandInterpriter>>();
            foreach (var _ownable in commands
                         .Cast<IOwnable<ICommandInterpriter>>())
                _ownable.ChangeOwner(this);
        }

        public virtual string Procces(string input)
        {
            foreach (var _ownable in pets)
                if (_ownable is ICommandInterpriter interpriter)
                {
                    var result = interpriter.Procces(input);
                    if (result != null)
                        return result;
                }

            return null;
        }


        ISet<IOwnable<ICommandInterpriter>> IOwner<ICommandInterpriter>.Pets => pets;

        public List<ICommandInterpriter> Owners { get; } = new();

        public static ICommandInterpriter GetRoot(ICommandInterpriter self)
        {
            var root = (IOwnable<ICommandInterpriter>)self as ICommandInterpriter;
            while (root.GetCurrentOwner() != null) root = root.GetCurrentOwner();

            return root;
        }

        public static IDebugSys<IGlobalGizmos, IMessenger> TryGetSys(ICommandInterpriter self)
        {
            var root = GetRoot(self);
            if (root is IMessenger messenger)
                if (root is IOwnable<IDebugSys<IGlobalGizmos, IMessenger>> ownable)
                    return ownable.GetCurrentOwner();


            return null;
        }

        public static string ReturnInvalidArgumentsExeption(IConsoleCommand command)
        {
            return $"The command {command.Name} had invalid arguments :( ";
        }

        public static bool TryGetBool(string input, out bool value)
        {
            if (bool.TryParse(input, out var result))
            {
                value = result;
                return true;
            }

            if (int.TryParse(input, out var result1))
            {
                var final = result1 > 0;
                value = final;
                return true;
            }

            value = false;
            return false;
        }

        public static bool CheckIfInputContainsCommand(string input, string command)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(command))
                return false;

            // Normalize: trim & lowercase
            input = input.Trim();
            command = command.Trim().ToLowerInvariant();

            // Escape special regex chars in command
            var escaped = Regex.Escape(command);

            // Regex pattern explanation:
            // ^\s*           — allow leading spaces
            // [!/:\-]?       — optional command prefix
            // (command)\b    — exact match of the command as a word
            // (?=\s|$|[.!?,]) — must end with space, EOL, or punctuation
            var pattern = $@"^\s*[!/:\-]?{escaped}\b(?=\s|$|[.!?,])";

            return Regex.IsMatch(input.ToLowerInvariant(), pattern);
        }

        public static List<string> ParseArguments(string input)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
                return result;

            // Remove the command prefix (like / or ! or :)
            input = input.Trim();

            // Regex explanation:
            // "([^"]*)" matches a quoted string
            // | ([^\s"]+) matches an unquoted word
            var matches = Regex.Matches(input, "\"([^\"]*)\"|([^\\s\"]+)");

            foreach (Match match in matches)
            {
                // Groups[1] is the quoted string, Groups[2] is the unquoted
                var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value);
            }

            // Remove the first token if it’s the command itself (starts with /)
            if (result.Count > 0 && result[0].StartsWith("/"))
                result.RemoveAt(0);

            return result;
        }

        public static string GetSingleCommandArgument(string input, string command)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(command))
                return null;

            input = input.Trim();

            // Split by spaces
            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return null; // no argument

            if (!parts[0].Equals(command, StringComparison.OrdinalIgnoreCase)
                && !parts[0].EndsWith(command, StringComparison.OrdinalIgnoreCase))
                return null; // command not found

            if (parts.Length > 2)
                return null; // too many arguments

            return parts[1]; // single argument
        }

        public class MatCommand : IConsoleCommand
        {
            public MatCommand()
            {
                Pets = new HashSet<IOwnable<ICommandInterpriter>> { new MatDebugViewCommand() };
                foreach (var _ownable in Pets) _ownable.ChangeOwner(this);
            }

            public ISet<IOwnable<ICommandInterpriter>> Pets { get; }
            public List<ICommandInterpriter> Owners { get; } = new();


            public string Procces(string input)
            {
                if (!input.Contains("mat"))
                    return null;
                foreach (var _ownable in Pets)
                {
                    if (_ownable is not IConsoleCommand cmd) continue;
                    if (!CheckIfInputContainsCommand(input, cmd.Name)) continue;
                    var final_input = input.Replace(cmd.Name, "");
                    return cmd.Procces(final_input);
                }

                return "use case: use a sub command with '_' prefix";
            }


            public string Name { get; set; } = "mat";
            public IMessage Description { get; set; } = IMessage.Literal("Changes the view of entities");

            public class MatDebugViewCommand : IConsoleCommand
            {
                public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
                public List<ICommandInterpriter> Owners { get; } = new();

                public string Procces(string input)
                {
                    var args = ParseArguments(input);
                    var sys = TryGetSys(this);
                    if (!args.IsInBounds(0) || string.IsNullOrEmpty(args[0]))
                        return null;

                    if (!args.IsInBounds(1) || string.IsNullOrEmpty(args[1]))
                        return null;
                    var layer = sys.Layers.FindByName(args[0]);
                    if (layer == default)
                        return $"layer {args[0]} was not found";
                    var val = false;
                    if (!TryGetBool(args[1], out val))
                        return $"layer value({args[1]}) was invalid";
                    (layer as DebugDrawLayer).Enabled = val;
                    return "debug draw updated";
                }

                public string Name { get; set; } = "mat_showdebug";

                public IMessage Description { get; set; } =
                    new CompText(new Span("Sets the debug draw layer's value").B_RT().I_RT());
            }
        }

        public class LoadLevelCommand : IConsoleCommand
        {
            private readonly ISaveSystem saveSystem;
            private readonly IRandom random;
            private readonly World world;
            public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
            public List<ICommandInterpriter> Owners { get; } = new();

            [Inject]
            public LoadLevelCommand(ISaveSystem saveSystem, IRandom random, World world)
            {
                this.saveSystem = saveSystem;
                this.random = random;
                this.world = world;
            }

            public string Procces(string input)
            {
                if (!CheckIfInputContainsCommand(input, Name))
                    return null;
                var result = GetSingleCommandArgument(input, Name);
                if (result == null)
                    return ReturnInvalidArgumentsExeption(this);
                if (int.TryParse(result, out var index))
                {
                    DefaultLevelTransition.Transition(index, saveSystem, world, random);
                    return $"Loading... {result}";
                }

                DefaultLevelTransition.Transition(result, saveSystem, world, random);
                return $"Loading... {result}";
            }


            public string Name { get; set; } = "map";
            public IMessage Description { get; set; } = IMessage.Literal("Changes the map");
        }

        public class ChangeSceneCommand : IConsoleCommand
        {
            public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
            public List<ICommandInterpriter> Owners { get; } = new();

            public string Procces(string input)
            {
                if (!CheckIfInputContainsCommand(input, Name))
                    return null;

                var result = GetSingleCommandArgument(input, Name);
                if (result == null)
                    return ReturnInvalidArgumentsExeption(this);
                if (int.TryParse(result, out var a))
                    SceneManager.LoadSceneAsync(a);
                else
                    SceneManager.LoadSceneAsync(result);

                return $"Changing scene...";
            }


            public string Name { get; set; } = "scene";

            public IMessage Description { get; set; } =
                IMessage.Literal("Changes the scene");
        }

        public class PlayAudioCommand : IConsoleCommand
        {
            private readonly IAudioSystem sys;
            public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
            public List<ICommandInterpriter> Owners { get; } = new();

            [Inject]
            public PlayAudioCommand(IAudioSystem sys)
            {
                this.sys = sys;
            }

            public string Procces(string input)
            {
                if (!CheckIfInputContainsCommand(input, Name))
                    return null;
                var result = GetSingleCommandArgument(input, Name);
                if (result == null)
                    return ReturnInvalidArgumentsExeption(this);

                sys.PlayOneShot(sys.GetDefault(), result);
                return $"playing {result} ...";
            }


            public string Name { get; set; } = "play";

            public IMessage Description { get; set; } =
                IMessage.Literal("plays a sound effect or music");
        }

        public class VersionCommand : IConsoleCommand
        {
            public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
            public List<ICommandInterpriter> Owners { get; } = new();

            public string Procces(string input)
            {
                if (!CheckIfInputContainsCommand(input, Name))
                    return null;
                if (GetSingleCommandArgument(input, Name) == "--unity")
                    return $"unity's version: {Application.unityVersion}";


                return $"version: {Application.version}";
            }


            public string Name { get; set; } = "version";

            public IMessage Description { get; set; } =
                IMessage.Literal("Displays the version of the game or unity (--unity)");
        }

        public class QuitCommand : IConsoleCommand
        {
            public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
            public List<ICommandInterpriter> Owners { get; } = new();

            public string Procces(string input)
            {
                if (!CheckIfInputContainsCommand(input, Name))
                    return null;

                Application.Quit();
                return "quiting now...";
            }


            public string Name { get; set; } = "q";
            public IMessage Description { get; set; } = IMessage.Literal("Exits from the game");
        }

        public class HelpCommand : IConsoleCommand
        {
            public ISet<IOwnable<ICommandInterpriter>> Pets { get; } = new HashSet<IOwnable<ICommandInterpriter>>();
            public List<ICommandInterpriter> Owners { get; } = new();

            public string Procces(string input)
            {
                if (!CheckIfInputContainsCommand(input, Name))
                    return null;


                List<IConsoleCommand> cmds = new();
                var root = GetRoot(this);

                GetPets(root, cmds);
                var commands = "";
                foreach (var _consoleCommand in cmds)
                    commands += $"{_consoleCommand.Name} : ({_consoleCommand.Description.Print()}) \n";

                return commands;
            }


            private void GetPets(ICommandInterpriter interpriter, List<IConsoleCommand> commands)
            {
                if (interpriter == default)
                    return;
                if (interpriter is IConsoleCommand cmd)
                    commands.Add(cmd);
                foreach (var _interpriterPet in interpriter.Pets)
                    if (_interpriterPet is ICommandInterpriter inter)
                        GetPets(inter, commands);
            }

            public string Name { get; set; } = "help";
            public IMessage Description { get; set; } = IMessage.Literal("Displays all commands");
        }
    }
}