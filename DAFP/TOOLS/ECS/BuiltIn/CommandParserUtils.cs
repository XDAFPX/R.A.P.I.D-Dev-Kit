using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using ModestTree;
using RapidLib.DAFP.TOOLS.Common;
using TripleA.Utils.Extensions;
using UGizmo;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public static class CommandParserUtils
    {
        public class ConsoleProgressBar : IProgress<float>
        {
            private readonly TextProcessContext _context;
            private readonly string label;
            private readonly char filling;
            private readonly char bounds;
            private readonly int len;
            private bool _started;

            public ConsoleProgressBar(TextProcessContext context, string label, char filling = '#', char bounds = '[',
                int len = 20)
            {
                _context = context;
                this.label = label;
                this.filling = filling;
                this.bounds = bounds;
                this.len = len;
            }


            public void Report(float progress)
            {
                int filled = (int)(progress * len);
                string bar =
                    $"{GetBracketPair(bounds).left}{new string(filling, filled)}{new string('-', len - filled)}{GetBracketPair(bounds).right}" +
                    $" {(int)(progress * 100)}% {label}";

                if (_started)
                    _context.Log.OnNext(IMessage.Literal("\x1b[1A" + bar));
                else
                {
                    _context.Log.OnNext(IMessage.Literal(bar));
                    _started = true;
                }
            }

            public static char[] BOUNDS_STYLES = new[] { '[', '<',  };
            public static char[] FILLING_STYLES = new[] { '=', '#', '%' };
        }

        public static IPlayer ParsePlayer(string plName, IEnumerable<IPlayer> players, out IMessage error)
        {
            error = null;
            var _enumerable = players as IPlayer[] ?? players.ToArray();
            _enumerable = _enumerable.Local().ToArray();

            if (_enumerable.IsEmpty())
            {
                error = NoPlayersException();
                return null;
            }

            if (!plName.IsNullOrEmpty())
            {
                var _player = _enumerable.FindByName(plName);
                if (_player == null)
                    error = IMessage.Literal($"Player '{plName}' does not exist");
                return _player; 
            }

            var _local = _enumerable.Local().FirstOrDefault() ?? _enumerable.FirstOrDefault();
            if (_local == null)
                error = NoPlayersException();
            return _local;
        }

        public static ICommandInterpreter GetRoot(ICommandInterpreter self)
        {
            var root = (IOwnedBy<ICommandInterpreter>)self as ICommandInterpreter;
            while (root.GetCurrentOwner() != null) root = root.GetCurrentOwner();
            return root;
        }

        public static IDebugSys<IGlobalGizmos, IConsoleMessenger> TryGetSys(ICommandInterpreter self)
        {
            var root = GetRoot(self);
            if (root is IConsoleMessenger messenger)
                if (root is IOwnedBy<IDebugSys<IGlobalGizmos, IConsoleMessenger>> ownable)
                    return ownable.GetCurrentOwner();
            return null;
        }

        public static IMessage GenericException()
            => IMessage.Literal($"Error happened");

        public static IMessage NoPlayersException()
            => IMessage.Literal($"There are no players in the game");

        public static IMessage InvalidArgumentsException(IConsoleCommand command)
            => IMessage.Literal($"The syntax of the command is incorrect.");

        public static bool TryGetBool(string input, out bool value)
        {
            if (bool.TryParse(input, out var result))
            {
                value = result;
                return true;
            }

            if (int.TryParse(input, out var result1))
            {
                value = result1 > 0;
                return true;
            }

            value = false;
            return false;
        }

        public static bool CheckIfInputContainsCommand(string input, string command)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(command))
                return false;
            input = input.Trim();
            command = command.Trim().ToLowerInvariant();
            var escaped = Regex.Escape(command);
            var pattern = $@"^\s*[!/:\-]?{escaped}\b(?=\s|$|[.!?,])";
            return Regex.IsMatch(input.ToLowerInvariant(), pattern);
        }

        public static List<string> ParseArguments(string input)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
                return result;
            input = input.Trim();
            var matches = Regex.Matches(input, "\"([^\"]*)\"|([^\\s\"]+)");
            foreach (Match match in matches)
            {
                var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value);
            }

            if (result.Count > 0 && result[0].StartsWith("/"))
                result.RemoveAt(0);
            return result;
        }

        public static string GetSingleCommandArgument(string input, string command)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(command))
                return null;
            input = input.Trim();
            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;
            if (!parts[0].Equals(command, StringComparison.OrdinalIgnoreCase)
                && !parts[0].EndsWith(command, StringComparison.OrdinalIgnoreCase))
                return null;
            if (parts.Length > 2) return null;
            return parts[1];
        }

        public static (char left, char right) GetBracketPair(char c)
        {
            return c switch
            {
                '(' or ')' => ('(', ')'),
                '[' or ']' => ('[', ']'),
                '{' or '}' => ('{', '}'),
                '<' or '>' => ('<', '>'),
                _ => throw new ArgumentException($"Unsupported bracket: {c}")
            };
        }
    }
}