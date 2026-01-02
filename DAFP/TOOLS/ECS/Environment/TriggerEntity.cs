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

namespace DAFP.TOOLS.ECS.Environment
{
    public class TriggerEntity : Entity
    {
        [SerializeField] private List<SerializableInterface<ITriggerFilter>> Filters;
        [SerializeField] private List<SerializableInterface<ITriggerAction>> Actions;


        public override NonEmptyList<IViewModel> SetupView()
        {
            return new NonEmptyList<IViewModel>(new EmptyView());
        }

        public override ITicker<IEntity> EntityTicker { get; } = Services.World.EMPTY_TICKER;

        private Collider2D col2d;
        private Collider col3d;

        protected override IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return base.SetupDebugDrawers().Union(new[]
            {
                new ActionDebugDrawer(DebugDrawLayer.DefaultDebugLayers.TRIGGERS, (
                    gizmos =>
                    {
                        if (col2d != null)
                            gizmos.DrawBox2D(col2d.bounds.center, 0, col2d.bounds.size, ColorsForUnity.Orange);

                        if (col3d != null)
                            gizmos.DrawCube(col2d.bounds.center, Quaternion.identity, col2d.bounds.size,
                                ColorsForUnity.Orange);
                    }))
            });
        }

        protected override void InitializeInternal()
        {
            if (TryGetComponent(out Collider2D _type)) col2d = _type;
            if (TryGetComponent(out Collider _tt)) col3d = _tt;
        }

        protected override void TickInternal()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            Eval(TriggerEvent.Enter, new TriggerCollider(other));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Eval(TriggerEvent.Enter, new TriggerCollider(other));
        }

        private void OnTriggerExit(Collider other)
        {
            Eval(TriggerEvent.Exit, new TriggerCollider(other));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Eval(TriggerEvent.Exit, new TriggerCollider(other));
        }

        public void Eval(TriggerEvent @event, TriggerCollider target)
        {
            if (!Filters.All((@interface =>
                {
                    @interface.Value.Event = @event;
                    return @interface.Value.Evaluate(target);
                })))
                return;
            Actions.ForEach((@interface => @interface.Value.Act(@event, target)));
        }

        public TriggerEntity AddAction(ITriggerAction filter)
        {
            Actions.Add(new SerializableInterface<ITriggerAction>(filter));
            return this;
        }

        public TriggerEntity RemoveAction(ITriggerAction filter)
        {
            Actions.Remove(new SerializableInterface<ITriggerAction>(filter));
            return this;
        }

        public TriggerEntity AddFilter(ITriggerFilter filter)
        {
            Filters.Add(new SerializableInterface<ITriggerFilter>(filter));
            return this;
        }

        public TriggerEntity RemoveFilter(ITriggerFilter filter)
        {
            Filters.Remove(new SerializableInterface<ITriggerFilter>(filter));
            return this;
        }
        [Flags]
        public enum TriggerEvent
        {
            None = 0,
            Enter =1 << 0,
            Exit = 2 <<1
        }
    }
}