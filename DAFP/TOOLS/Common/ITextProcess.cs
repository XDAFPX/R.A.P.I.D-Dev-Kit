using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
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
        public static ITextProcess Literal(string msg) => new LiteralProcess(IMessage.Literal(msg));

        private class LiteralProcess : ITextProcess
        {
            private readonly IMessage msg;

            public LiteralProcess(IMessage msg)
            {
                this.msg = msg;
            }

            public UniTask Execute(TextProcessContext context, CancellationToken ct)
            {
                context.Log.OnNext(msg);
                return UniTask.CompletedTask;
            }

            public string Name { get; set; } = "Literal Process";
        }
    }


    public class TextProcessContext
    {
        public Subject<IMessage> Log { get; set; }
    }
}