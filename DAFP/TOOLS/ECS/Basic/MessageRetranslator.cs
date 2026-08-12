using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using FluentResults;
using MessagePipe;
using Optional.Unsafe;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Basic
{
    public class MessageRetranslator<TIn, TOut> : IDisposable, IInitializable
    {
        private readonly Func<TIn, Result<TOut>> syncTranslator;
        private readonly Func<TIn, CancellationToken, UniTask<Result<TOut>>> asyncTranslator;
        private readonly bool isAsyncIn;
        private readonly bool isAsyncOut;
        private readonly bool silentOnFailure;

        [Inject] private ISubscriber<TIn> syncSubscriber;
        [Inject] private IAsyncSubscriber<TIn> asyncSubscriber;
        [Inject] private IPublisher<TOut> syncPublisher;
        [Inject] private IAsyncPublisher<TOut> asyncPublisher;

        private IDisposable sub;

        // default — uses ResolveAs<TOut> as translator, optionally silent on failure
        public MessageRetranslator(bool asyncIn = false, bool asyncOut = false, bool silentOnFailure = true)
        {
            this.silentOnFailure = silentOnFailure;
            isAsyncIn = asyncIn;
            
            isAsyncOut = asyncOut;

            syncTranslator = msg =>
            {
                var result = GameUtils.ResolveAs<TOut>(msg);
                return result.HasValue
                    ? Result.Ok(result.ValueOrFailure())
                    : Result.Fail("Could not resolve");
            };

            if (asyncIn)
                asyncTranslator = (msg, ct) => UniTask.FromResult(syncTranslator(msg));
        }

        
        // sync → sync / sync → async
        public MessageRetranslator(Func<TIn, Result<TOut>> translator, bool asyncOut = false,
            bool silentOnFailure = true)
        {
            this.silentOnFailure = silentOnFailure;
            syncTranslator = translator;
            isAsyncIn = false;
            isAsyncOut = asyncOut;
        }

        // async → sync / async → async
        public MessageRetranslator(Func<TIn, CancellationToken, UniTask<Result<TOut>>> translator,
            bool asyncOut = false, bool silentOnFailure = true)
        {
            this.silentOnFailure = silentOnFailure;
            asyncTranslator = translator;
            isAsyncIn = true;
            isAsyncOut = asyncOut;
        }

        public void Initialize()
        {
            var bag = DisposableBag.CreateBuilder();
            if (isAsyncIn)
                asyncSubscriber.Subscribe(HandleAsync).AddTo(bag);
            else
                syncSubscriber.Subscribe(HandleSync).AddTo(bag);
            sub = bag.Build();
        }

        private void HandleSync(TIn message)
        {
            var result = syncTranslator.Invoke(message);
            if (!result.IsSuccess)
            {
                if (!silentOnFailure)
                    Debug.LogWarning(
                        $"[Retranslator] {typeof(TIn).Name} → {typeof(TOut).Name} failed: {result.Errors}");
                return;
            }

            if (isAsyncOut)
                asyncPublisher.PublishAsync(result.Value).Forget();
            else
                syncPublisher.Publish(result.Value);
        }

        private async UniTask HandleAsync(TIn message, CancellationToken ct)
        {
            var result = await asyncTranslator.Invoke(message, ct);
            if (!result.IsSuccess)
            {
                if (!silentOnFailure)
                    Debug.LogWarning(
                        $"[Retranslator] {typeof(TIn).Name} → {typeof(TOut).Name} failed: {result.Errors}");
                return;
            }

            if (isAsyncOut)
                await asyncPublisher.PublishAsync(result.Value, ct);
            else
                syncPublisher.Publish(result.Value);
        }

        public void Dispose() => sub.Dispose();
    }
}