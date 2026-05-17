using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using DAFP.TOOLS.ECS.Components;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components.Movement
{
    [RequireComponent(typeof(Mover2D))]
    [RequireComponent(typeof(UniversalCooldownController))]
    public class UniversalJumper2D
        : UniversalJumperBase<Vector2>
    {
        [GetComponent] private Mover2D mover;

        protected override Vector2 UpVector => transform.up;

        protected override Vector2 MultiplyVector(Vector2 vec, float m)
        {
            return vec * m;
        }

        protected override void PerformJump(Vector2 force)
        {
            mover.DoJump(force);
        }

        protected override void PerformCutJump(Vector2 up, float divisor)
        {
            mover.DoCutJump(up, divisor);
        }
    }
}