using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.ECS.BuiltIn;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public abstract class ConsoleCommand : IConsoleCommand //--changed signature
    {
        protected IConsoleMessenger  Console => CommandParserUtils.GetRoot(this) as IConsoleMessenger;
        public List<ICommandInterpreter> Children { get; } = new();
        public List<ICommandInterpreter> Owners { get; } = new();
        public abstract string Name { get; set; }
        public abstract IMessage Description { get; set; }
        protected string SourceInput = String.Empty;

        public ITextProcess Process(string input)
        {
            if (!CommandParserUtils.CheckIfInputContainsCommand(input.ToLower(), Name)) return null;
            SourceInput = input;
            return this;
        }

        public abstract UniTask Execute(TextProcessContext context, CancellationToken ct);
    }
}