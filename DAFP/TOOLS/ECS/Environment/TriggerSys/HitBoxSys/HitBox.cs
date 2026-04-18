using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.ViewModel;
using RapidLib.DAFP.TOOLS.Aspects;
using RapidLib.DAFP.TOOLS.Common;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Events;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    // public interface ICollidableContext<T>
    // {
    //     public T Build(TriggerCollider coll);
    // }


    public abstract class HitBox<T> : CollidableFilterActionEntity<T>, INameable, IAct
    {
        protected override Color DebugColor => Color.softRed;

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
            
            
            BroadcastEvent( new HitBoxActivatedEvent()
            {
                StuffCaught = _enumerable.Cast<object>().ToArray(), Hitbox = this,
                Owner = ((IOwnedBy<IEntity>)this).GetCurrentOwner()
            });
            
            BroadcastEvent(new HitBoxActivatedEvent<T>()
            {
                ActionsDone = Actions.ToValues().ToArray(),
                FiltersActivated = Filters.ToValues().ToArray(),
                HurtBoxesFound = _hurtBoxes,
                StuffCaught = _enumerable, Hitbox = this,
                Owner = ((IOwnedBy<IEntity>)this).GetCurrentOwner()
            });
        }

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            foreach (var _filter in Filters.ToValues())
            {
                _filter.Initialize(((IOwnedBy<IEntity>)this).GetCurrentOwner());
            }
        }

        protected abstract IEnumerable<T> BuildContext(IEnumerable<HurtBox<T>> hits);

        private IEnumerable<HurtBox<T>> get_boxes(IEnumerable<TriggerCollider> colliders)
        {
            foreach (var _triggerCollider in colliders)
            {
                if (_triggerCollider.GameObject == null)
                    continue;
                if (_triggerCollider.GameObject.TryGetComponent<HurtBox<T>>(out var _hurtBox))
                {
                    yield return _hurtBox;
                }
            }
        }


        private List<TriggerCollider> collect_all_hits()
        {
            var _seen2d = new HashSet<Collider2D>();
            var _seen3d = new HashSet<Collider>();
            var _result = new List<TriggerCollider>();

            var _filter = new ContactFilter2D { useTriggers = true };

            foreach (var _col in Cols2d)
            {
                var _hits = new List<Collider2D>();
                _col.Overlap(_filter, _hits);
                foreach (var _hit in _hits)
                {
                    if (!_seen2d.Add(_hit)) continue;
                    _result.Add(new TriggerCollider(_hit));
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
                    _result.Add(new TriggerCollider(_hit));
                }
            }

            return _result;
        }

        private static readonly Collider[] buffer3d = new Collider[64];
    }

    public struct HitBoxActivatedEvent<T>
    {
        public HitBox<T> Hitbox;
        public IEntity Owner;
        public IFilter<T>[] FiltersActivated;
        public IActionUpon<T>[] ActionsDone;
        public T[] StuffCaught;
        public HurtBox<T>[] HurtBoxesFound;
    }

    public struct HitBoxActivatedEvent
    {
        public IEntity Hitbox;
        public IEntity Owner;
        public object[] StuffCaught;
    }
}