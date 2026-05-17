using System;
using DAFP.TOOLS.Common.Utill;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    [Serializable]
    public class QuakeAirMovementOverride : IMovementOverride
    {
        [field: Priority]
        [field: SerializeField]
        public int Priority { get; set; }


        public OverrideResult OnInputMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            var _wishVelocity = IMovementOverride.ToVec(inputMovement).Normalized;
            float _wishSpeed = ctx.MovementSpeed;

            // if (wish_speed > 30)
            //     wish_speed = 30;
            var _currentSpeed = ctx.Velocity.DotProduct(_wishVelocity);

            float _addSpeed = _wishSpeed - _currentSpeed;
            if (_addSpeed <= 0)
                return OverrideResult.Suppress;
            var _accelSpeed = ctx.MovementSpeed * ctx.Acceleration * ctx.DeltaTime;
            if (_accelSpeed > _addSpeed)
                _accelSpeed = _addSpeed;

            ctx.Actions.IntegrateForce(_wishVelocity.Scale(_accelSpeed));

            return OverrideResult.Suppress;
        }

        public OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            return OverrideResult.Continue;
        }
    }
}