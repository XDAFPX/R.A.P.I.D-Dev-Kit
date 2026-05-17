using System;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    [Serializable]
    public class QuakeGroundMovementOverride : IMovementOverride
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
            var _addspeed = _pushvec.Magnitude;
            var _accelspeed = ctx.Acceleration * ctx.DeltaTime * _addspeed;

            _pushvec = _pushvec.Normalize();

            if (_accelspeed > _addspeed) _accelspeed = _addspeed;

            ctx.Actions.IntegrateForce(_pushvec.Scale(_accelspeed), false);
            return OverrideResult.Suppress;
        }

        public OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            return OverrideResult.Continue;
        }
    }
}