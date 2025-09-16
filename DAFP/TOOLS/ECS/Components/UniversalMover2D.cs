using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CanMoveBoard))]
    [RequireComponent(typeof(IsStunnedBoard))]
    [RequireComponent(typeof(MovementSpeedBoard))]
    public class UniversalMover2D 
        : UniversalMoverBase<Vector2, Rigidbody2D, ForceMode2D>
    {
        protected override float GetMass()
        {
            return rb.mass;
        }

        protected override Vector2 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        protected override void SetVelocity(Vector2 v) => rb.linearVelocity = v;

        protected override void AddForce(Vector2 f, ForceMode2D m) => rb.AddForce(f, m);
        protected override void AddForce(Vector2 f) => rb.AddForce(f);
        protected override ForceMode2D DefaultForceMode() => ForceMode2D.Force;
        protected override ForceMode2D ImpulseMode()      => ForceMode2D.Impulse;

        protected override Vector2 ZeroVector => Vector2.zero;
        protected override int Dimension       => 2;
        protected override Vector2 Multiply(Vector2 v, float s) => v * s;
        protected override Vector2 Divide(Vector2 v, float s)   => v / s;
        protected override float GetComponent(Vector2 v, int a) => a == 0 ? v.x : v.y;
        protected override Vector2 SetComponent(Vector2 v, int a, float x)
            => a == 0 ? new Vector2(x, v.y) : new Vector2(v.x, x);
        protected override Vector2 MaskOutAxis(Vector2 v, int a)
            => a == 1 ? new Vector2(v.x, 0) : v;
        protected override float Magnitude(Vector2 v) => v.magnitude;
        protected override float Angle(Vector2 a, Vector2 b) => Vector2.Angle(a, b);
    }
}
