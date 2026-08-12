using UnityEngine;
using NRandom;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.Common
{
    /// <summary>
    /// Randomizes a Rigidbody or Rigidbody2D's velocity, angular velocity, and optionally
    /// physical properties, using UniversalRigidbody so one component works for both.
    /// </summary>
    [DisallowMultipleComponent]
    public class RigidbodyRandomizer : MonoBehaviour, IRandomizer
    {
        private UniversalRigidbody? _rb;

        private UniversalRigidbody Rb
        {
            get
            {
                if (_rb.HasValue) return _rb.Value;

                var rb3D = GetComponent<Rigidbody>();
                if (rb3D != null)
                {
                    _rb = new UniversalRigidbody(rb3D);
                    return _rb.Value;
                }

                var rb2D = GetComponent<Rigidbody2D>();
                if (rb2D != null)
                {
                    _rb = new UniversalRigidbody(rb2D);
                    return _rb.Value;
                }

                Debug.LogWarning($"[{nameof(RigidbodyRandomizer)}] No Rigidbody or Rigidbody2D found on {name}.", this);
                _rb = new UniversalRigidbody((Rigidbody)null);
                return _rb.Value;
            }
        }

        [Header("Linear Velocity")] [SerializeField]
        private bool randomizeVelocity = false;

        [Tooltip("If true, picks a random direction (circle for 2D, sphere for 3D) and a random speed. " +
                 "If false, randomizes each axis independently within its own range.")]
        [SerializeField]
        private bool velocityAsDirectionAndSpeed = true;

        [SerializeField] private float speedMin = 0f;
        [SerializeField] private float speedMax = 5f;
        [SerializeField] private Vector2 velocityXRange = new Vector2(-5f, 5f);
        [SerializeField] private Vector2 velocityYRange = new Vector2(-5f, 5f);
        [SerializeField] private Vector2 velocityZRange = new Vector2(-5f, 5f); // ignored for 2D

        [Header("Angular Velocity")] [SerializeField]
        private bool randomizeAngularVelocity = false;

        [SerializeField] private float angularSpeedMin = 0f;
        [SerializeField] private float angularSpeedMax = 180f;

        [Header("Physical Properties (optional)")] [SerializeField]
        private bool randomizeMass = false;

        [SerializeField] private float massMin = 0.5f;
        [SerializeField] private float massMax = 2f;

        [SerializeField] private bool randomizeDrag = false;
        [SerializeField] private float dragMin = 0f;
        [SerializeField] private float dragMax = 2f;

        [SerializeField] private bool randomizeAngularDrag = false;
        [SerializeField] private float angularDragMin = 0f;
        [SerializeField] private float angularDragMax = 2f;

        [Tooltip("2D bodies only. Ignored for 3D.")] [SerializeField]
        private bool randomizeGravityScale = false;

        [SerializeField] private float gravityScaleMin = 0f;
        [SerializeField] private float gravityScaleMax = 2f;

        public void Randomize(IRandom rng)
        {
            if (rng == null)
            {
                Debug.LogWarning($"[{nameof(RigidbodyRandomizer)}] Randomize called with null rng on {name}.", this);
                return;
            }
            
            
            var body = Rb; // <-- cache once

            
            if (randomizeVelocity) RandomizeVelocity(rng);
            if (randomizeAngularVelocity) RandomizeAngularVelocity(rng);
            if (randomizeMass) body.mass = rng.Range(massMin, massMax);
            if (randomizeDrag) body.drag = rng.Range(dragMin, dragMax);
            if (randomizeAngularDrag) body.angularDrag = rng.Range(angularDragMin, angularDragMax);
            if (randomizeGravityScale && body.Is2D) body.gravityScale = rng.Range(gravityScaleMin, gravityScaleMax);
        }

        // ---------------- Velocity ----------------

        
        public void RandomizeVelocity(IRandom rng)
        {
            var body = Rb;
            

            if (velocityAsDirectionAndSpeed)
            {
                float speed = rng.Range(speedMin, speedMax);

                if (body.Is2D)
                {
                    float angle = rng.Range(0f, 360f) * Mathf.Deg2Rad;
                    IVector dir = new V2(Mathf.Cos(angle), Mathf.Sin(angle));
                    body.velocity = ((V2)dir).Scale(speed);
                }
                else
                {
                    IVector dir = RandomUnitVector3(rng);
                    body.velocity = ((V3)dir).Scale(speed);
                }
            }
            else
            {
                if (body.Is2D)
                {
                    body.velocity = new V2(
                        rng.Range(velocityXRange.x, velocityXRange.y),
                        rng.Range(velocityYRange.x, velocityYRange.y));
                }
                else
                {
                    body.velocity = new V3(
                        rng.Range(velocityXRange.x, velocityXRange.y),
                        rng.Range(velocityYRange.x, velocityYRange.y),
                        rng.Range(velocityZRange.x, velocityZRange.y));
                }
            }
        }

        /// <summary>Utility: random direction, custom speed range, bypassing serialized fields.</summary>
        public void RandomizeVelocity(IRandom rng, float minSpeed, float maxSpeed)
        {
            var body = Rb;
            float speed = rng.Range(minSpeed, maxSpeed);

            if (body.Is2D)
            {
                float angle = rng.Range(0f, 360f) * Mathf.Deg2Rad;
                body.velocity = new V2(Mathf.Cos(angle), Mathf.Sin(angle)).Scale(speed);
            }
            else
            {
                body.velocity = RandomUnitVector3(rng).Scale(speed);
            }
        }

        // ---------------- Angular Velocity ----------------

        public void RandomizeAngularVelocity(IRandom rng)
        {
            var body = Rb;
            float speed = rng.Range(angularSpeedMin, angularSpeedMax);

            if (body.Is2D)
            {
                // 2D only rotates around Z; sign gives direction (CW/CCW).
                float signed = rng.Range(0, 2) == 0 ? -speed : speed;
                body.angularVelocity = new V3(0f, 0f, signed);
            }
            else
            {
                V3 axis = RandomUnitVector3(rng);
                body.angularVelocity = axis.Scale(speed);
            }
        }

        // ---------------- Helpers ----------------

        /// <summary>Uniformly distributed random unit vector on the 3D sphere.</summary>
        private static V3 RandomUnitVector3(IRandom rng)
        {
            float z = rng.Range(-1f, 1f);
            float theta = rng.Range(0f, Mathf.PI * 2f);
            float r = Mathf.Sqrt(1f - z * z);
            return new V3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), z);
        }
    }
}