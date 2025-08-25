using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CanMoveBoard))]
    [RequireComponent(typeof(IsStunnedBoard))]
    [RequireComponent(typeof(MovementSpeedBoard))]
    public class UniversalMover3D 
        : UniversalMoverBase<Vector3, Rigidbody, ForceMode>
    {
        protected override Vector3 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        protected override void SetVelocity(Vector3 v) => rb.linearVelocity = v;

        protected override void AddForce(Vector3 f, ForceMode m) => rb.AddForce(f, m);
        protected override void AddForce(Vector3 f) => rb.AddForce(f);
        protected override ForceMode DefaultForceMode() => ForceMode.Force;
        protected override ForceMode ImpulseMode()      => ForceMode.Impulse;

        protected override Vector3 ZeroVector => Vector3.zero;
        protected override int Dimension       => 3;
        protected override Vector3 Multiply(Vector3 v, float s) => v * s;
        protected override Vector3 Divide(Vector3 v, float s)   => v / s;
        protected override float GetComponent(Vector3 v, int a)
            => a == 0 ? v.x : (a == 1 ? v.y : v.z);
        protected override Vector3 SetComponent(Vector3 v, int a, float x)
            => a == 0 ? new Vector3(x, v.y, v.z)
             : a == 1 ? new Vector3(v.x, x, v.z)
             : new Vector3(v.x, v.y, x);
        protected override Vector3 MaskOutAxis(Vector3 v, int a)
            => a == 1 ? new Vector3(v.x, 0, v.z) : v;
        protected override float Magnitude(Vector3 v) => v.magnitude;
        protected override float Angle(Vector3 a, Vector3 b) => Vector3.Angle(a, b);
    }
}
