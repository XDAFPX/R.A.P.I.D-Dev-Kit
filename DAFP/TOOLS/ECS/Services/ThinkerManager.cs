using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.Injection;
using JetBrains.Annotations;
using MessagePipe;
using R3;
using RapidLib.DAFP.TOOLS.Common.Utill;
using UnityEngine;
using UnityEngine.Scripting;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    [Preserve]
    public sealed class ThinkerManager : IMessageHandler<OnEntityRegisterEvent>,
        IMessageHandler<OnEntityDeregisterEvent>, IMessageHandler<OnEntityThinkerChangedEvent>, IDisposable,
        IInitializable
    {
        [Inject] private ISubscriber<OnEntityRegisterEvent> reg;
        [Inject] private IPublisher<OnEntityThinkerChangedEvent> thinkerChanged;
        [Inject] private ISubscriber<OnEntityThinkerChangedEvent> thinkerChangedsub;
        [Inject] private ISubscriber<OnEntityDeregisterEvent> dereg;

        [Inject] private Adam adam;
        private IDisposable sub;

        [Inject(Id = IVideoGame.THINKERS_UPDATE)]
        private ITicker thinkerUpdate;

        public void Initialize()
        {
            var _d1 = reg.Subscribe(this);
            var _d2 = dereg.Subscribe(this);
            var _d3 = thinkerChangedsub.Subscribe(this);
            sub = new CompositeDisposable(_d1, _d2, _d3);
        }


        void IMessageHandler<OnEntityRegisterEvent>.Handle(OnEntityRegisterEvent message)
        {
            if (message.Entity is not IEntityLogic _logic) return;
            _logic.ThinkerTicker = new BrainTicker(message.Entity, thinkerUpdate);
            thinkerUpdate.Subscribed.Add(_logic.ThinkerTicker);

            prepare_thinker(message.Entity);
        }

        void IMessageHandler<OnEntityDeregisterEvent>.Handle(OnEntityDeregisterEvent message)
        {
            if (message.Entity is not IEntityLogic _logic) return;

            thinkerUpdate.Subscribed.Remove(_logic.ThinkerTicker);
        }

        public void Handle(OnEntityThinkerChangedEvent message)
        {
            start_brains(message.Entity);
        }


        /// <summary>
        /// Swaps the brains of two entities: <paramref name="a"/> receives <paramref name="b"/>'s brain
        /// and vice versa. Both entities must implement <see cref="IEntityLogic"/> and already have a brain.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown if <paramref name="a"/> or <paramref name="b"/> has no brain.
        /// </exception>
        /// <returns><c>false</c> without swapping if either entity doesn't support brain-swapping.</returns>
        public bool SwapBrains(IEntity a, IEntity b)
        {
            if (a.Brains == null)
                throw new Exception($"[{nameof(ThinkerManager)}] :: Can't swap brains, entity {a} has none.");
            if (b.Brains == null)
                throw new Exception($"[{nameof(ThinkerManager)}] :: Can't swap brains, entity {b} has none.");

            if (a is not IEntityLogic aLogic) return false;
            if (b is not IEntityLogic bLogic) return false;

            var aBrain = a.Brains;
            var bBrain = b.Brains;

            aLogic.SetThinker(bBrain);
            bLogic.SetThinker(aBrain);

            prepare_thinker(a);
            prepare_thinker(b);

            return true;
        }

        /// <summary>
        /// Assigns <paramref name="thinker"/> as the brain for <paramref name="ent"/>.
        /// </summary>
        /// <remarks>
        /// <b>Destructive:</b> if <paramref name="ent"/> already has a brain, it is permanently
        /// destroyed. Do not use this to swap brains between entities — use <see cref="SwapBrains"/> instead.
        /// </remarks>
        public void SetBrain([CanBeNull] IThinker thinker, [NotNull] IEntity ent)
        {
            if (ent is not IEntityLogic logic) return;

            if (ent.Brains != null && ent.Brains is not DummyThinker && ent.Brains is IThinkerLogic _logic)
            {
                _logic.End(ent);
                adam.Destroy(ent.Brains).Forget();
            }

            logic.SetThinker(thinker);
            prepare_thinker(ent);
        }


        private void prepare_thinker(IEntity ent)
        {
            wake_thinker(ent);
            thinkerChanged.Publish(new(ent));
        }

        private void wake_thinker(IEntity ent)
        {
            if (ent.Brains == null || ent.Brains is DummyThinker) return;
            if (ent is not IEntityLogic _logic) return;
            if (ent.Brains is not IThinkerLogic { HasWoken: false }) return;
            ensure_woke(ent, _logic);
        }

        private void ensure_woke(IEntity ent, IEntityLogic _logic)
        {
            adam.Create<IThinker>(Adam.CreationInfo.New<IThinker>(new ThinkerCreationInfo(ent.Brains)))
                .ContinueWith((_logic.SetThinker))
                .Forget();
        }


        private void start_brains(IEntity ent)
        {
            if (ent.Brains is not IThinkerLogic _logic) return;
            _logic.Start(ent);
        }


        private void end_brains(IEntity ent)
        {
            if (ent.Brains is not IThinkerLogic _logic) return;
            _logic.End(ent);
        }
        //
        // private IThinker deep_clone(IThinker original)
        // {
        //     if (original is Brain { EditMode: true })
        //         return original;
        //     if (original is not ScriptableObject _so)
        //         return original;
        //     return (IThinker)_so.DeepClone();
        // }
        //
        // private void deep_destroy(IThinker original)
        // {
        //     if (original is not ScriptableObject _so)
        //         return;
        //     _so.DeepDestroy();
        // }


        public void Dispose()
        {
            sub.Dispose();
        }
    }

    internal struct ThinkerCreationInfo : Adam.IObjectAssetCreationInfo
    {
        public ThinkerCreationInfo(IThinker original)
        {
            Original = original;
        }

        public IThinker Original { get; }
    }
}