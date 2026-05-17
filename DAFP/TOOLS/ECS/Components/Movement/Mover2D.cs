using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components.Movement
{
    using DAFP.TOOLS.Common.Maths;
    [RequireComponent(typeof(Rigidbody2D))]
    public class Mover2D
        : MoverBase<Vector2, Rigidbody2D, ForceMode2D>
    {
        protected override Vector2 Normalize(Vector2 vec)
        {
            return vec.normalized;
        }

        protected override float DotProduct(Vector2 vec1, Vector2 vec2)
        {
            return Vector2.Dot(vec1, vec2);
        }

        protected override float GetMass()
        {
            return Rb.mass;
        }

        protected override Vector2 Velocity
        {
            get => Rb.linearVelocity;
            set => Rb.linearVelocity = value;
        }

        protected override void SetVelocity(Vector2 v)
        {
            Rb.linearVelocity = v;
        }

        protected override void InternalAddForce(Vector2 f, ForceMode2D m)
        {
            Rb.AddForce(f, m);
        }


        protected override ForceMode2D DefaultForceMode()
        {
            return ForceMode2D.Force;
        }

        protected override ForceMode2D ImpulseMode()
        {
            return ForceMode2D.Impulse;
        }

        protected override Vector2 ZeroVector => Vector2.zero;
        protected override int Dimension => 2;

        protected override Vector2 Multiply(Vector2 v, float s)
        {
            return v * s;
        }

        protected override Vector2 Subtract(Vector2 v, Vector2 v1)
        {
            return v - v1;
        }

        protected override Vector2 Add(Vector2 v, Vector2 v1)
        {
            return v + v1;
        }

        protected override Vector2 CurrentPos()
        {
            return transform.position;
        }

        protected override Vector2 Divide(Vector2 v, float s)
        {
            return v / s;
        }

        protected override float GetComponent(Vector2 v, int a)
        {
            return a == 0 ? v.x : v.y;
        }

        protected override Vector2 SetComponent(Vector2 v, int a, float x)
        {
            return a == 0 ? new Vector2(x, v.y) : new Vector2(v.x, x);
        }

        protected override Vector2 MaskOutAxis(Vector2 v, int a)
        {
            return a == 1 ? new Vector2(v.x, 0) : v;
        }

        protected override Vector3 GetVec3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        protected override float Magnitude(Vector2 v)
        {
            return v.magnitude;
        }

        protected override float Angle(Vector2 a, Vector2 b)
        {
            return Vector2.Angle(a, b);
        }

        protected override Vector2 FromIVector(IVector v)
        {
            // Use TryGetVector2 helper; implicit cast to Vector2 via V2
            V2 v2 = v.TryGetVector2();
            return (Vector2)v2;
        }

        protected override IVector ToIVector(Vector2 v)
        {
            return v.ToGeneric();
        }
    }
}