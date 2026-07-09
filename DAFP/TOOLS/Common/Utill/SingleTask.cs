using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Common.Utill
{
    public class SingleTask
    {
        private CancellationTokenSource _cts;

        public void Run(Func<CancellationToken, UniTask> factory)
        {
            var old = _cts;
            _cts = new CancellationTokenSource();
            factory(_cts.Token).Forget(ex => Debug.LogError(ex));
            old?.Cancel(); // cancel AFTER new task is already set up
            old?.Dispose();
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }
}