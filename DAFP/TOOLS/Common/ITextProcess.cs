using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.ECS.BuiltIn;
using R3;

namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IProcess<in TProcessContext> : INameable
    {
        UniTask Execute(TProcessContext context, CancellationToken ct);
    }

    public interface ITextProcess : IProcess<TextProcessContext>
    {
        public static ITextProcess Literal(IMessage msg) => new LiteralProcess(msg);
        public static ITextProcess Empty => new EmptyProcess();
        public static ITextProcess Literal(string msg) => new LiteralProcess(IMessage.Literal(msg));

        public static ITextProcess Literal(Func<TextProcessContext, CancellationToken ,UniTask> exec) =>
            new LiteralProcess(exec);

        private class EmptyProcess : ITextProcess
        {
            private readonly IMessage msg;


            public UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public string Name { get; set; } = "Empty Process";
        }

        private class LiteralProcess : ITextProcess
        {
            private readonly Func<TextProcessContext, CancellationToken, UniTask> exec;
            private readonly IMessage msg;

            public LiteralProcess(Func<TextProcessContext,CancellationToken, UniTask> exec, string name = "Literal Process")
            {
                Name = name;
                this.exec = exec;
            }

            public LiteralProcess(IMessage msg, string name = "Literal Process")
            {
                Name = name;
                this.msg = msg;
            }

            public async UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                if (exec != null)
                    await exec.Invoke(context,ct);

                context.Log.OnNext(msg);
            }

            public string Name { get; set; }
        }
    }


    public class TextProcessContext
    {
        public Subject<IMessage> Log { get; set; }
    }
}