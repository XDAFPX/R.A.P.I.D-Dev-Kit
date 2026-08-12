using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.Environment.Filters;
using MessagePipe;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    // public interface ICollidableContext<T>
    // {
    //     public T Build(TriggerCollider coll);
    // }


    public abstract class HitBox<T> : CollidableFilterActionEntity<T>, INameable, IAct,ITechnicalEntity
    {
        protected override Color DebugColor => Color.softRed;
        [Inject] private IPublisher<OnHitBoxActivatedEvent> e;
        [Inject] private IPublisher<OnHitBoxActivatedEvent<T>> genericEvent;

        public void Act()
        {
            var _hits = collect_all_hits();
            var boxes = get_boxes(_hits);

            var _hurtBoxes = boxes as HurtBox<T>[] ?? boxes.ToArray();

            //-- Flag as boxes being touched/hit 
            _hurtBoxes.ForEach((box => box.FlagAsHit()));


            var ctx = BuildContext(_hurtBoxes);
            var _enumerable = ctx as T[] ?? ctx.ToArray();

            Eval(_enumerable);


            //----- EVENTS


            var inst1 = new OnHitBoxActivatedEvent()
            {
                StuffCaught = _enumerable.Cast<object>().ToArray(), Hitbox = this,
                Owner = ((IOwnedBy<IEntity>)this).GetCurrentOwner()
            };
            e.Publish(inst1);


            var inst2 = new OnHitBoxActivatedEvent<T>()
            {
                ActionsDone = Actions.ToValues().ToArray(),
                FiltersActivated = Filters.ToValues().ToArray(),
                HurtBoxesFound = _hurtBoxes,
                StuffCaught = _enumerable, Hitbox = this,
                Owner = ((IOwnedBy<IEntity>)this).GetCurrentOwner()
            };
            genericEvent.Publish(inst2);
        }

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            // Filters are stateless now; no Initialize needed.
        }

        protected abstract IEnumerable<T> BuildContext(IEnumerable<HurtBox<T>> hits);

        private IEnumerable<HurtBox<T>> get_boxes(IEnumerable<UniversalCollider> colliders)
        {
            foreach (var _triggerCollider in colliders)
            {
                if (_triggerCollider.gameObject == null)
                    continue;
                if (_triggerCollider.gameObject.TryGetComponent<HurtBox<T>>(out var _hurtBox))
                {
                    yield return _hurtBox;
                }
            }
        }


        private List<UniversalCollider> collect_all_hits()
        {
            var _seen2d = new HashSet<Collider2D>();
            var _seen3d = new HashSet<Collider>();
            var _result = new List<UniversalCollider>();

            var _filter = new ContactFilter2D { useTriggers = true };

            foreach (var _col in Cols2d)
            {
                var _hits = new List<Collider2D>();
                _col.Overlap(_filter, _hits);
                foreach (var _hit in _hits)
                {
                    if (!_seen2d.Add(_hit)) continue;
                    _result.Add(new UniversalCollider(_hit));
                }
            }

            foreach (var _col in Cols3d)
            {
                var _count = Physics.OverlapBoxNonAlloc(
                    _col.bounds.center,
                    _col.bounds.extents,
                    buffer3d,
                    _col.transform.rotation,
                    Physics.AllLayers,
                    QueryTriggerInteraction.Collide // <-- include triggers
                );
                for (int _i = 0; _i < _count; _i++)
                {
                    var _hit = buffer3d[_i];
                    if (_hit == _col) continue;
                    if (!_seen3d.Add(_hit)) continue;
                    _result.Add(new UniversalCollider(_hit));
                }
            }

            return _result;
        }

        private static readonly Collider[] buffer3d = new Collider[64];
    }
}