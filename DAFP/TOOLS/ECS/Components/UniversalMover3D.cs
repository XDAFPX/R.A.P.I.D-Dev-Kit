using DAFP.TOOLS.ECS.BigData.Common;
using UnityEngine;
using Zenject.Internal;

namespace DAFP.TOOLS.ECS.Components
{
    using DAFP.TOOLS.Common.Maths;
    [RequireComponent(typeof(Rigidbody))]
    public class UniversalMover3D
        : UniversalMoverBase<Vector3, Rigidbody, ForceMode>
    {
        protected override Vector3 Normalize(Vector3 vec)
        {
            return Vector3.Normalize(vec);
        }

        protected override float DotProduct(Vector3 vec1, Vector3 vec2)
        {
            return Vector3.Dot(vec1, vec2);
        }

        protected override float GetMass()
        {
            return Rb.mass;
        }

        protected override Vector3 Velocity
        {
            get => Rb.linearVelocity;
            set => Rb.linearVelocity = value;
        }

        protected override void SetVelocity(Vector3 v)
        {
            Rb.linearVelocity = v;
        }

        public override void AddForce(Vector3 f, ForceMode m)
        {
            Rb.AddForce(f, m);
        }

        protected override void AddForce(Vector3 f)
        {
            Rb.AddForce(f);
        }

        protected override ForceMode DefaultForceMode()
        {
            return ForceMode.Force;
        }

        protected override ForceMode ImpulseMode()
        {
            return ForceMode.Impulse;
        }

        protected override Vector3 ZeroVector => Vector3.zero;
        protected override int Dimension => 3;

        protected override Vector3 Multiply(Vector3 v, float s)
        {
            return v * s;
        }

        protected override Vector3 Subtract(Vector3 v, Vector3 v1)
        {
            return v - v1;
        }

        protected override Vector3 Add(Vector3 v, Vector3 v1)
        {
            return v + v1;
        }

        protected override Vector3 CurrentPos()
        {
            return transform.position;
        }

        protected override Vector3 Divide(Vector3 v, float s)
        {
            return v / s;
        }

        protected override float GetComponent(Vector3 v, int a)
        {
            return a == 0 ? v.x : (a == 1 ? v.y : v.z);
        }

        protected override Vector3 SetComponent(Vector3 v, int a, float x)
        {
            return a == 0 ? new Vector3(x, v.y, v.z)
                : a == 1 ? new Vector3(v.x, x, v.z)
                : new Vector3(v.x, v.y, x);
        }

        protected override Vector3 MaskOutAxis(Vector3 v, int a)
        {
            return a == 1 ? new Vector3(v.x, 0, v.z) : v;
        }

        protected override Vector3 GetVec3(Vector3 v)
        {
            return v;
        }

        protected override float Magnitude(Vector3 v)
        {
            return v.magnitude;
        }

        protected override float Angle(Vector3 a, Vector3 b)
        {
            return Vector3.Angle(a, b);
        }

        protected override Vector3 FromIVector(IVectorBase v)
        {
            V3 v3 = v.TryGetVector3();
            return (Vector3)v3;
        }
    }
}