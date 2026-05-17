using System;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    public interface IMovementOverride : IPriority<IMovementOverride>
    {
        /// <summary>
        /// Full replacement for handle_input_movement.
        /// Only called when a previous override (or this one) returned Suppress on OnBeforeInputMovement.
        /// Use this to plug in GroundAccel / AirAccel etc.
        /// </summary>
        OverrideResult OnInputMovement<TVec>(MoverContext ctx, ref TVec inputMovement);

        /// <summary>
        /// Called after default input movement ran (or after replacement).
        /// Good place for additive effects (ice friction, speed boosts).
        /// </summary>
        OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement);


        // -------------------------------------------------------------------------
        // COMMAND HOOKS  (ref params let you mutate force/time before they execute)
        // -------------------------------------------------------------------------

        OverrideResult OnBeforeJump<TVec>(MoverContext ctx, ref TVec jump)
            => OverrideResult.Continue;

        OverrideResult OnBeforeDash<TVec>(MoverContext ctx, ref TVec force, ref float time)
            => OverrideResult.Continue;

        OverrideResult OnBeforeKnockback<TVec>(MoverContext ctx, ref TVec force, ref float time, ref float delay)
            => OverrideResult.Continue;

        OverrideResult OnBeforeHalt(MoverContext ctx, ref float divisor)
            => OverrideResult.Continue;

        OverrideResult OnBeforeCutJump<TVec>(MoverContext ctx, ref TVec positive, ref float multiplier)
            => OverrideResult.Continue;

        OverrideResult OnBeforeAddForce<TVec>(MoverContext ctx, ref TVec force, bool isImpulse)
            => OverrideResult.Continue;

        public static IVector ToVec(object thing)
        {
            switch (thing)
            {
                case Vector2 _v2:
                    return _v2.ToGeneric();
                case Vector3 _v3:
                    return _v3.ToGeneric();
                case Vector4 _v4:
                    return _v4.ToGeneric();
                default:
                    throw new ArgumentException("Unexpected Movement Vector ");
            }
        }
    }
}