using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using DAFP.TOOLS.ECS.Components;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    [RequireComponent(typeof(JumpStrengthBoard))]
    [RequireComponent(typeof(UniversalMover3D))]
    [RequireComponent(typeof(UniversalCooldownController))]
    public class UniversalJumper3D 
        : UniversalJumperBase<Vector3>
    {
        [GetComponentCache] private UniversalMover3D universalMover;

        protected override Vector3 UpVector => transform.up;

        protected override Vector3 MultiplyVector(Vector3 vec, float m)
        {
            return vec * m;
        }

        protected override void PerformJump(Vector3 force)
        {
            universalMover.DoJump(force);
        }

        protected override void PerformCutJump(Vector3 up, float divisor)
        {
            universalMover.DoCutJump(up, divisor);
        }
    }
}
