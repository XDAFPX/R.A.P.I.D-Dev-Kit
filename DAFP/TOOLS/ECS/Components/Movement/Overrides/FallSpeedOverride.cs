using System;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    [System.Serializable]
    public class FallSpeedMovementOverride : IMovementOverride
    {
        [field: Priority]
        [field: SerializeField]
        public int Priority { get; set; }

        [SerializeField] protected float MaxFallSpeed = 50;

        public OverrideResult OnInputMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            return OverrideResult.Continue;
        }

        public OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            var _y = ctx.Velocity.GetValueAtDimension(2);
            if (!_y.HasValue || !(_y < -MaxFallSpeed))
                return OverrideResult.Continue;

            var _velocity = ctx.Velocity.SetValueAtDimension(2, -MaxFallSpeed);
            ctx.Actions.SetVelocity(_velocity);

            return OverrideResult.Continue;
        }


    }
}