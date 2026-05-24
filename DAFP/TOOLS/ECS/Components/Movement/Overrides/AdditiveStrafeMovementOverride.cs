using System;
using DAFP.TOOLS.Common.Utill;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    [Serializable]
    public class AdditiveStrafeMovementOverride : IMovementOverride
    {
        [field: SerializeField]
        [field: Priority]
        public int Priority { get; set; }

        [SerializeField] private float Force = 0.1f;

        public OverrideResult OnInputMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            var _input = IMovementOverride.ToVec(inputMovement).Normalized;
            var _curVel = ctx.Velocity.Normalized;

            if (_input.Magnitude < Mathf.Epsilon) return OverrideResult.Continue;

            var _addedSpeed = 1 - Mathf.Abs(_input.Dot(_curVel));

            var _finalSpeed = _addedSpeed * ctx.Acceleration * Force;


            var _finalVec = _curVel.Add(_input).Scale(_finalSpeed);

            ctx.Actions.IntegrateForce(_finalVec);


            return OverrideResult.Continue;
        }

        public OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            return OverrideResult.Continue;
        }
    }
}