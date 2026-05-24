using System;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    [Serializable]
    public class ReactiveInputMovementOverride : IMovementOverride
    {
        [field: Priority]
        [field: SerializeField]
        public int Priority { get; set; }

        [SerializeField] private float WaitTrashold = 0.5f;
        [SerializeField] private float Power = 5f;
        private float curTime;

        public OverrideResult OnInputMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            var vec = IMovementOverride.ToVec(inputMovement);

            if (vec.Magnitude < Mathf.Epsilon)
                curTime += ctx.DeltaTime;
            else if (curTime > WaitTrashold)
            {
                ctx.Actions.IntegrateForce(vec.Scale(Power), true);
                curTime = 0;
            }


            return OverrideResult.Continue;
        }

        public OverrideResult OnPostMovement<TVec>(MoverContext ctx, ref TVec inputMovement)
        {
            return OverrideResult.Continue;
        }
    }
}