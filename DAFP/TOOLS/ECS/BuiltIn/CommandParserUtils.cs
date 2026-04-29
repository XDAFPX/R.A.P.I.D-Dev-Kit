using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using ModestTree;
using UGizmo;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public static class CommandParserUtils
    {
        public static IPlayer ParsePlayer(string plName, IEnumerable<IPlayer> players, out string error)
        {
            error = null;
            var _enumerable = players as IPlayer[] ?? players.ToArray();

            if (_enumerable.IsEmpty())
            {
                error = NoPlayersException();
                return null;
            }

            if (plName != null)
            {
                var _player = _enumerable.FindByName(plName);
                if (_player == null)
                    error = $"Player '{plName}' does not exist";
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

        public static string GenericException()
            => $"Error happened";

        public static string NoPlayersException()
            => $"There are no players in the game";

        public static string InvalidArgumentsException(IConsoleCommand command)
            => $"The syntax of the command is incorrect.";

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
    }
}