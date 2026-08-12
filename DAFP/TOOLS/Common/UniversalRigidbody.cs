using UnityEngine;
using DAFP.TOOLS.Common.Maths;

namespace DAFP.TOOLS.ECS.Environment
{
    /// <summary>
    /// Wrapper that can represent either a 3D Rigidbody or a 2D Rigidbody2D in a unified way.
    /// </summary>
    public readonly struct UniversalRigidbody
    {
        public readonly Rigidbody Rigidbody3D;
        public readonly Rigidbody2D Rigidbody2D;

        public UniversalRigidbody(Rigidbody rigidbody)
        {
            Rigidbody2D = null;
            Rigidbody3D = rigidbody;
        }

        public UniversalRigidbody(Rigidbody2D rigidbody2D)
        {
            Rigidbody3D = null;
            Rigidbody2D = rigidbody2D;
        }

        public bool Is2D => Rigidbody2D != null;
        public bool Is3D => Rigidbody3D != null;

        public Component component => (Component)Rigidbody3D ? (Component)Rigidbody3D : Rigidbody2D;

        public GameObject gameObject =>
            Is3D ? Rigidbody3D.gameObject : Rigidbody2D != null ? Rigidbody2D.gameObject : null;

        public Transform transform =>
            Is3D ? Rigidbody3D.transform : Rigidbody2D != null ? Rigidbody2D.transform : null;

        public bool enabled
        {
            get
            {
                if (Rigidbody2D != null) return Rigidbody2D.simulated;
                if (Rigidbody3D != null) return !Rigidbody3D.isKinematic;
                return false;
            }
            set
            {
                if (Rigidbody2D != null) Rigidbody2D.simulated = value;
                if (Rigidbody3D != null) Rigidbody3D.isKinematic = !value;
            }
        }

        public float mass
        {
            get => Is3D ? Rigidbody3D.mass : Rigidbody2D != null ? Rigidbody2D.mass : 0f;
            set
            {
                if (Rigidbody3D != null) Rigidbody3D.mass = value;
                if (Rigidbody2D != null) Rigidbody2D.mass = value;
            }
        }

        /// <summary>Linear drag / damping, unified across 2D (linearDamping) and 3D (linearDamping).</summary>
        public float drag
        {
            get => Is3D ? Rigidbody3D.linearDamping : Rigidbody2D != null ? Rigidbody2D.linearDamping : 0f;
            set
            {
                if (Rigidbody3D != null) Rigidbody3D.linearDamping = value;
                if (Rigidbody2D != null) Rigidbody2D.linearDamping = value;
            }
        }

        public float angularDrag
        {
            get => Is3D ? Rigidbody3D.angularDamping : Rigidbody2D != null ? Rigidbody2D.angularDamping : 0f;
            set
            {
                if (Rigidbody3D != null) Rigidbody3D.angularDamping = value;
                if (Rigidbody2D != null) Rigidbody2D.angularDamping = value;
            }
        }

        /// <summary>2D-only. Reading/writing on a 3D body is a no-op / returns 1.</summary>
        public float gravityScale
        {
            get => Rigidbody2D != null ? Rigidbody2D.gravityScale : 1f;
            set
            {
                if (Rigidbody2D != null) Rigidbody2D.gravityScale = value;
            }
        }

        /// <summary>
        /// Linear velocity as an IVector: V2 for a 2D body, V3 for a 3D body.
        /// Setting with the "wrong" dimension count simply drops the extra/missing axis.
        /// </summary>
        public IVector velocity
        {
            get
            {
                if (Rigidbody2D != null) return (V2)Rigidbody2D.linearVelocity;
                if (Rigidbody3D != null) return (V3)Rigidbody3D.linearVelocity;
                return default(V3);
            }
            set
            {
                if (Rigidbody2D != null)
                {
                    Vector2 v2 = new Vector2(
                        value.GetValueAtDimension(1) ?? 0f,
                        value.GetValueAtDimension(2) ?? 0f);
                    Rigidbody2D.linearVelocity = v2;
                }
                else if (Rigidbody3D != null)
                {
                    Vector3 v3 = new Vector3(
                        value.GetValueAtDimension(1) ?? 0f,
                        value.GetValueAtDimension(2) ?? 0f,
                        value.GetValueAtDimension(3) ?? 0f);
                    Rigidbody3D.linearVelocity = v3;
                }
            }
        }

        /// <summary>
        /// Angular velocity as an IVector: 2D bodies only rotate around Z, so it's
        /// represented as V3(0, 0, degreesPerSecond) for a uniform interface;
        /// 3D bodies use the full V3 axis*speed vector.
        /// </summary>
        public IVector angularVelocity
        {
            get
            {
                if (Rigidbody2D != null) return new V3(0f, 0f, Rigidbody2D.angularVelocity);
                if (Rigidbody3D != null) return (V3)Rigidbody3D.angularVelocity;
                return default(V3);
            }
            set
            {
                if (Rigidbody2D != null)
                {
                    Rigidbody2D.angularVelocity = value.GetValueAtDimension(3) ?? 0f;
                }
                else if (Rigidbody3D != null)
                {
                    Vector3 v3 = new Vector3(
                        value.GetValueAtDimension(1) ?? 0f,
                        value.GetValueAtDimension(2) ?? 0f,
                        value.GetValueAtDimension(3) ?? 0f);
                    Rigidbody3D.angularVelocity = v3;
                }
            }
        }
// --- Add to UniversalRigidbody ---

        /// <summary>
        /// Applies a force using Unity's 3D ForceMode semantics, translated to the
        /// 2D equivalent when this wraps a Rigidbody2D. ForceMode2D only has
        /// Force/Impulse, so Acceleration/VelocityChange collapse to their closest match:
        /// Acceleration -> Force, VelocityChange -> Impulse.
        /// </summary>
        public void AddForce(IVector force, ForceMode mode = ForceMode.Force)
        {
            if (Is2D)
            {
                Vector2 f2 = new Vector2(
                    force.GetValueAtDimension(1) ?? 0f,
                    force.GetValueAtDimension(2) ?? 0f);

                ForceMode2D mode2D = (mode == ForceMode.Impulse || mode == ForceMode.VelocityChange)
                    ? ForceMode2D.Impulse
                    : ForceMode2D.Force;

                Rigidbody2D.AddForce(f2, mode2D);
            }
            else if (Is3D)
            {
                Vector3 f3 = new Vector3(
                    force.GetValueAtDimension(1) ?? 0f,
                    force.GetValueAtDimension(2) ?? 0f,
                    force.GetValueAtDimension(3) ?? 0f);

                Rigidbody3D.AddForce(f3, mode);
            }
        }

        /// <summary>Explicit 2D overload for exact ForceMode2D control (no translation).</summary>
        public void AddForce(IVector force, ForceMode2D mode2D)
        {
            if (!Is2D)
            {
                AddForce(force, ForceMode.Force);
                return;
            } // fall back sensibly on 3D

            Vector2 f2 = new Vector2(
                force.GetValueAtDimension(1) ?? 0f,
                force.GetValueAtDimension(2) ?? 0f);

            Rigidbody2D.AddForce(f2, mode2D);
        }

        /// <summary>
        /// Applies a force at a world-space point (torque-inducing). 2D uses AddForceAtPosition
        /// with only X/Y taken from the point; 3D uses the full 3-axis point.
        /// </summary>
        public void AddForceAtPosition(IVector force, IVector worldPosition, ForceMode mode = ForceMode.Force)
        {
            if (Is2D)
            {
                Vector2 f2 = new Vector2(force.GetValueAtDimension(1) ?? 0f, force.GetValueAtDimension(2) ?? 0f);
                Vector2 p2 = new Vector2(worldPosition.GetValueAtDimension(1) ?? 0f,
                    worldPosition.GetValueAtDimension(2) ?? 0f);

                ForceMode2D mode2D = (mode == ForceMode.Impulse || mode == ForceMode.VelocityChange)
                    ? ForceMode2D.Impulse
                    : ForceMode2D.Force;

                Rigidbody2D.AddForceAtPosition(f2, p2, mode2D);
            }
            else if (Is3D)
            {
                Vector3 f3 = new Vector3(
                    force.GetValueAtDimension(1) ?? 0f,
                    force.GetValueAtDimension(2) ?? 0f,
                    force.GetValueAtDimension(3) ?? 0f);

                Vector3 p3 = new Vector3(
                    worldPosition.GetValueAtDimension(1) ?? 0f,
                    worldPosition.GetValueAtDimension(2) ?? 0f,
                    worldPosition.GetValueAtDimension(3) ?? 0f);

                Rigidbody3D.AddForceAtPosition(f3, p3, mode);
            }
        }

        /// <summary>
        /// Applies torque. 2D torque is a single scalar (rotation around Z); pass an IVector
        /// and only dimension 3 (z) is read, matching the angularVelocity convention above.
        /// 3D reads the full 3-axis torque vector.
        /// </summary>
        public void AddTorque(IVector torque, ForceMode mode = ForceMode.Force)
        {
            if (Is2D)
            {
                float t = torque.GetValueAtDimension(3) ?? 0f;

                ForceMode2D mode2D = (mode == ForceMode.Impulse || mode == ForceMode.VelocityChange)
                    ? ForceMode2D.Impulse
                    : ForceMode2D.Force;

                Rigidbody2D.AddTorque(t, mode2D);
            }
            else if (Is3D)
            {
                Vector3 t3 = new Vector3(
                    torque.GetValueAtDimension(1) ?? 0f,
                    torque.GetValueAtDimension(2) ?? 0f,
                    torque.GetValueAtDimension(3) ?? 0f);

                Rigidbody3D.AddTorque(t3, mode);
            }
        }

        /// <summary>Convenience overload: torque as a single scalar (Z-axis for 2D, or magnitude around world-up isn't assumed for 3D — use the IVector overload for 3D torque direction).</summary>
        public void AddTorque(float torqueZOrScalar, ForceMode mode = ForceMode.Force)
        {
            if (Is2D)
            {
                ForceMode2D mode2D = (mode == ForceMode.Impulse || mode == ForceMode.VelocityChange)
                    ? ForceMode2D.Impulse
                    : ForceMode2D.Force;
                Rigidbody2D.AddTorque(torqueZOrScalar, mode2D);
            }
            // Intentionally no 3D fallback here — a bare scalar has no unambiguous axis in 3D.
        }

        public static implicit operator UniversalRigidbody(Rigidbody rb) => new UniversalRigidbody(rb);
        public static implicit operator UniversalRigidbody(Rigidbody2D rb2d) => new UniversalRigidbody(rb2d);
    }
}