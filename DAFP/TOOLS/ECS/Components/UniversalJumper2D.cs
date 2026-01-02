using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using DAFP.TOOLS.ECS.Components;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    [RequireComponent(typeof(UniversalMover2D))]
    [RequireComponent(typeof(UniversalCooldownController))]
    public class UniversalJumper2D
        : UniversalJumperBase<Vector2>
    {
        [GetComponentCache] private UniversalMover2D universalMover;

        protected override Vector2 UpVector => transform.up;

        protected override Vector2 MultiplyVector(Vector2 vec, float m)
        {
            return vec * m;
        }

        protected override void PerformJump(Vector2 force)
        {
            universalMover.DoJump(force);
        }

        protected override void PerformCutJump(Vector2 up, float divisor)
        {
            universalMover.DoCutJump(up, divisor);
        }
    }
}