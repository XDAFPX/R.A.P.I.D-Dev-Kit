using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using DAFP.TOOLS.ECS.Components;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components.Movement
{
    [RequireComponent(typeof(Mover3D))]
    [RequireComponent(typeof(UniversalCooldownController))]
    public class UniversalJumper3D
        : UniversalJumperBase<Vector3>
    {
        [GetComponent] private Mover3D mover;

        protected override Vector3 UpVector => transform.up;

        protected override Vector3 MultiplyVector(Vector3 vec, float m)
        {
            return vec * m;
        }

        protected override void PerformJump(Vector3 force)
        {
            mover.DoJump(force);
        }

        protected override void PerformCutJump(Vector3 up, float divisor)
        {
            mover.DoCutJump(up, divisor);
        }
    }
}