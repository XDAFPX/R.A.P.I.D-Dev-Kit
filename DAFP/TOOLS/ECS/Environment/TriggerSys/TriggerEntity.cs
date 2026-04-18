using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.ViewModel;
using PixelRouge.Colors;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys
{
    public class TriggerEntity : CollidableFilterActionEntity<TriggerContext>
    {
        private void OnTriggerEnter(Collider other) => handle(TriggerEvent.Enter, new TriggerCollider(other));
        private void OnTriggerEnter2D(Collider2D other) => handle(TriggerEvent.Enter, new TriggerCollider(other));
        private void OnTriggerExit(Collider other) => handle(TriggerEvent.Exit, new TriggerCollider(other));
        private void OnTriggerExit2D(Collider2D other) => handle(TriggerEvent.Exit, new TriggerCollider(other));

        private void handle(TriggerEvent triggerEvent, TriggerCollider collider)
        {
            var ctx = new TriggerContext(triggerEvent, collider);
            Eval(ctx);
            BroadcastEvent(new TriggerActivatedEvent() { TriggerEntity = this, Ctx = ctx });
        }


        [Flags]
        public enum TriggerEvent
        {
            None = 0,
            Enter = 1 << 0,
            Exit = 2 << 1
        }

        public struct TriggerActivatedEvent
        {
            public TriggerEntity TriggerEntity;
            public TriggerContext Ctx;
        }
    }


    [Flags]
    public enum TriggerEvent
    {
        None = 0,
        Enter = 1 << 0,
        Exit = 2 << 1
    }

    public struct TriggerActivatedEvent
    {
        public TriggerEntity TriggerEntity;
        public TriggerContext Ctx;
    }
}
