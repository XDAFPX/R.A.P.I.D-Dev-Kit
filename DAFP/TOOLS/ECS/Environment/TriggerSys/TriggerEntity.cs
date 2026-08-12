using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.ViewModel;
using MessagePipe;
using PixelRouge.Colors;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys
{
    public class TriggerEntity : CollidableFilterActionEntity<TriggerContext>,ITechnicalEntity
    {
        [Inject] private IPublisher<OnTriggerActivatedEvent> e;
        private void OnTriggerEnter(Collider other) => handle(TriggerEvent.Enter, new UniversalCollider(other));
        private void OnTriggerEnter2D(Collider2D other) => handle(TriggerEvent.Enter, new UniversalCollider(other));
        private void OnTriggerExit(Collider other) => handle(TriggerEvent.Exit, new UniversalCollider(other));
        private void OnTriggerExit2D(Collider2D other) => handle(TriggerEvent.Exit, new UniversalCollider(other));

        private void handle(TriggerEvent triggerEvent, UniversalCollider collider)
        {
            var ctx = new TriggerContext(triggerEvent, collider);
            Eval(ctx);
            e.Publish(new OnTriggerActivatedEvent() { TriggerEntity = this, Ctx = ctx });
        }


        [Flags]
        public enum TriggerEvent
        {
            None = 0,
            Enter = 1 << 0,
            Exit = 2 << 1
        }

    }


    [Flags]
    public enum TriggerEvent
    {
        None = 0,
        Enter = 1 << 0,
        Exit = 2 << 1
    }

}
