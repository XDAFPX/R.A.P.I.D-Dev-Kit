using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RapidLib.DAFP.TOOLS.Common.Utill
{
    public class SingleTask
    {
        private CancellationTokenSource _cts;

        public void Run(Func<CancellationToken, UniTask> factory)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            factory(_cts.Token).Forget();
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }

}