using DAFP.TOOLS.Common.Utill;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    public class FishMovementOverride : IMovementOverride
    {
        [field: Priority]
        [field: SerializeField]
        public int Priority { get; set; }

        public OverrideResult OnInputMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            if (ctx.MovementSpeed == 0) return OverrideResult.Suppress;
            var _input = IMovementOverride.ToVec(inputMovement);
            var _wishvel = _input.Scale(ctx.MovementSpeed);
            var _pushvec = _wishvel.Subtract(ctx.Velocity);

            // Project push onto wish direction (the "get up to speed" part)
            // and the remainder is perpendicular (the "kill old momentum" part)
            var _wishDir = _wishvel.Normalized;
            var _parallelMag = _pushvec.DotProduct(_wishDir);
            var _parallel = _wishDir.Scale(_parallelMag);
            var _perp = _pushvec.Subtract(_parallel);

            // Decel multiplier makes direction changes snappier
            // 1f = original ground accel feel, 3-5f = very snappy
            var _perpForce = _perp.Scale(ctx.Deceleration * ctx.DeltaTime);
            var _parallelForce = _parallel.Scale(ctx.Acceleration * ctx.DeltaTime);

            // Clamp parallel so we don't overshoot wish speed
            var _addspeed = _parallelForce.Magnitude;
            var _accelspeed = ctx.Acceleration * ctx.DeltaTime * _addspeed;
            if (_accelspeed > _addspeed) _accelspeed = _addspeed;

            var _totalForce = _wishDir.Scale(_accelspeed).Add(_perpForce);
            ctx.Actions.IntegrateForce(_totalForce, false);
            return OverrideResult.Suppress;
        }

        public OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            return OverrideResult.Continue;
        }
    }
}