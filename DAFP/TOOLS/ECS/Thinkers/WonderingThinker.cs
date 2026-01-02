using System.Collections.Generic;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Thinkers;
using NRandom;
using Optional;
using PixelRouge.Colors;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Thinkers
{
    [CreateAssetMenu(menuName = "R.A.P.I.D/BuiltIn/Thinker/" + nameof(WonderingThinker),fileName = nameof(WonderingThinker))]
    public class WonderingThinker : BaseThinker
    {
        [Inject] private IRandom random;
        [SerializeField] private float RadiusOfSearch = 10;
        [SerializeField] private float ReachedTargetRadius = 1;

        protected override void InternalInitialize(IEntity host)
        {
            if (host is ICommonEntityInterface.IEntTargetContainable _targetContainable)
            {
                if (_targetContainable.Target.HasValue)
                    _targetContainable.ResetTarget();
                update_target(_targetContainable);
            }
        }

        private void update_target(ICommonEntityInterface.IEntTargetContainable host)
        {
            if (host.Target.TryGetValue(out var target))
            {
                Destroy(target.GetWorldRepresentation());
                host.Target = Option.None<IEntity>();
            }

            tryToAssignNewTarget.reset();
            if (try_gen_new_waypoint(host).TryGetValue(out var val))
            {
                var world = val.GetWorldRepresentation(); 

                if (world == null)
                    return;

                var pos = world.transform.position;

                val.AddPet(new ActionDebugDrawer("Thinkers",
                    gizmos => { gizmos.DrawCircle2D(pos, ReachedTargetRadius, ColorsForUnity.FireBrick); }));

                host.Target = val.Some();
            }
        }

        private Option<IEntity> try_gen_new_waypoint(IEntity host) =>
            host.TryToFindARandomWaypoint2D(random, RadiusOfSearch).TryGetValue(out Vector2 _val)
                ? World.SpawnEmptyEntity(_val).Some()
                : Option.None<IEntity>();

        private FiniteTimer tryToAssignNewTarget = new FiniteTimer(2);
        private FiniteTimer walkToTarget = new FiniteTimer(10);

        protected override void InternalTick(IEntity host, ITickerBase ticker)
        {
            tryToAssignNewTarget.safeUpdateTimer(ticker.DeltaTime);
            if (host is ICommonEntityInterface.IEntTargetContainable _targetContainable)
            {
                if (!_targetContainable.Target.HasValue && tryToAssignNewTarget.isComplete)
                    update_target(_targetContainable);
                if (_targetContainable.Target.TryGetValue(out var _target))
                {
                    try_to_walk_to_target(_targetContainable, _target, ticker.DeltaTime);
                }
            }
        }

        private void try_to_walk_to_target(ICommonEntityInterface.IEntTargetContainable host, IEntity target,
            float delta)
        {
            if (host is not ICommonEntityInterface.IEntMovementInputable _movementInputable) return;
            walkToTarget.updateTimer(delta);


            if (check_for_distance()) return;


            if (check_for_timer()) return;

            if (host is ICommonEntityInterface.IEntPathFindable _pathFindable)
            {
                _movementInputable.InputMovement(_pathFindable.PathFindToTarget().Normalized);
                return;
            }

            _movementInputable.InputMovement((V3)(target.GetWorldRepresentation().transform.position -
                                                  host.GetWorldRepresentation().transform.position).normalized);

            bool check_for_timer()
            {
                if (walkToTarget.isComplete)
                {
                    walkToTarget.reset();
                    update_target(host);
                    return true;
                }

                return false;
            }

            bool check_for_distance()
            {
                if (Vector3.Distance(target.GetWorldRepresentation().transform.position,
                        host.GetWorldRepresentation().transform.position) <= ReachedTargetRadius)
                {
                    update_target(host);
                    return true;
                }

                return false;
            }
        }

        protected override void InternalDispose(IEntity host)
        {
        }

        protected override IEnumerable<IDebugDrawer> SetupDebugDrawers(IEntity host)
        {
            return new[]
            {
                new ActionDebugDrawer("Thinkers", (gizmos => gizmos.DrawWireCircle2D(
                    host.GetWorldRepresentation().transform.position,
                    RadiusOfSearch, ColorsForUnity.Azure)))
            };
        }
    }
}