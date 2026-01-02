using System;

namespace DAFP.TOOLS.ECS.Components.GrabController
{
    using UnityEngine;

    /// <summary>
    /// Handles the logic for attaching, carrying, and detaching physics objects.
    /// </summary>
    [Serializable]
    public class GrabController : IGrabController
    {
        [field: SerializeField] public Rigidbody AttachedRigidbody { get; set; }
        private GameObject AttachedObject { get; set; }

        private Vector3 localGrabOffset;
        private Quaternion localGrabRotation;
        private float savedMass;
        private float savedDrag;
        private float savedAngularDrag;

        private CarrySettings carrySettings;
        private bool hasPreferredAngles;
        private Quaternion preferredRotation;
        private float carryDistanceOffset;

        [field: SerializeField] public float DefaultHoldDistance { get; set; }
        [field: SerializeField] public float MassWhileCarring { get; set; }
        [field: SerializeField] public float Damping { get; set; }
        [field: SerializeField] public float MaxExitVelocity { get; set; }
        public Rigidbody AttachedBody => AttachedRigidbody;
        [field: SerializeField] public Transform Cam { get; set; }
        public event Action<Rigidbody> OnAttached;
        public event Action<Rigidbody> OnDeattached;


        public GrabController(Transform camera, float defaultHoldDistance = 2f, float massWhileCarring = 10f,
            float damping = 10f)
        {
            Cam = camera;
            DefaultHoldDistance = defaultHoldDistance;
            MassWhileCarring = massWhileCarring;
            Damping = damping;
        }

        public void Attach(GameObject target)
        {
            var rb = target.GetComponent<Rigidbody>();
            if (rb == null) return;
            // Save old physics properties
            savedMass = rb.mass;
            savedDrag = rb.linearDamping;
            savedAngularDrag = rb.angularDamping;

            AttachedRigidbody = rb;
            AttachedObject = target;

            // Check for carry settings
            carrySettings = target.GetComponent<CarrySettings>();
            hasPreferredAngles = carrySettings && carrySettings.usePreferredAngles;
            carryDistanceOffset = carrySettings ? carrySettings.distanceOffset : 0f;

            // Reduce mass to avoid crazy energy transfer
            rb.mass = MassWhileCarring;
            rb.linearDamping = Damping;
            rb.angularDamping = Damping;

            // Compute local offsets so the object stays aligned
            localGrabOffset = rb.transform.InverseTransformPoint(Cam.position +
                                                                 Cam.forward * (DefaultHoldDistance +
                                                                     carryDistanceOffset));
            localGrabRotation = Quaternion.Inverse(rb.transform.rotation) * Cam.rotation;

            if (hasPreferredAngles) preferredRotation = Quaternion.Euler(carrySettings.preferredCarryAngles);

            OnAttached?.Invoke(rb);
        }

        public void Detach()
        {
            if (AttachedRigidbody != null)
            {
                // Restore old mass and damping
                AttachedRigidbody.mass = savedMass;
                AttachedRigidbody.linearDamping = savedDrag;
                AttachedRigidbody.angularDamping = savedAngularDrag;

                if (MaxExitVelocity == 0f)
                {
                    AttachedRigidbody.linearVelocity = Vector3.zero;
                    AttachedRigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    var normalized_vel = AttachedBody.linearVelocity.magnitude / AttachedBody.mass;
                    var normalized = AttachedBody.linearVelocity.normalized;
                    normalized_vel = Mathf.Clamp(normalized_vel, 0, MaxExitVelocity);
                    AttachedBody.linearVelocity = normalized * (normalized_vel * AttachedBody.mass);
                }

                AttachedRigidbody = null;
                AttachedObject = null;
                carrySettings = null;

                OnDeattached?.Invoke(AttachedRigidbody);
            }
        }


        /// <summary>
        /// Call this each FixedUpdate from your MonoBehaviour.
        /// </summary>
        public void Simulate()
        {
            if (AttachedRigidbody == null) return;

            // Target position
            var targetPos = Cam.position +
                            Cam.forward * (DefaultHoldDistance + carryDistanceOffset);

            // Target rotation
            Quaternion targetRot;
            if (hasPreferredAngles)
                targetRot = Cam.rotation * preferredRotation;
            else
                targetRot = Cam.rotation * Quaternion.Inverse(localGrabRotation);

            // Position correction
            var posError = targetPos - AttachedRigidbody.position;
            var velTarget = posError * 20f;
            var velChange = velTarget - AttachedRigidbody.linearVelocity;
            AttachedRigidbody.AddForce(velChange, ForceMode.VelocityChange);

            // Rotation correction
            var rotError = targetRot * Quaternion.Inverse(AttachedRigidbody.rotation);
            rotError.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f) angle -= 360f;

            if (Mathf.Abs(angle) > 0.01f)
            {
                var angularVelTarget = axis * angle * Mathf.Deg2Rad * 10f;
                var angularVelChange = angularVelTarget - AttachedRigidbody.angularVelocity;
                AttachedRigidbody.AddTorque(angularVelChange, ForceMode.VelocityChange);
            }
        }
    }
}