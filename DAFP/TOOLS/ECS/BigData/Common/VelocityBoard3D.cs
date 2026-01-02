using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class VelocityBoard3D : Vector3Board
    {
        private Rigidbody rb;

        [Range(0, 100)] [SerializeField] private int FrameBufferSize = 10;

        public readonly Queue<Vector3> VelocityHistory = new();

        [Min(0)] [SerializeField] private float TopVelocity;

        [Min(0)] [SerializeField] private float MinVelocity;

        public override Vector3 MinValue => Vector3.one * MinVelocity;
        public override Vector3 MaxValue => Vector3.one * TopVelocity;

        protected override void OnInitializeInternal()
        {
            rb = Host.GetWorldRepresentation().GetComponent<Rigidbody>();
            if (rb == null)
                return;

            InternalValue = rb.linearVelocity;
            DefaultValue = rb.linearVelocity;
        }


        public override void SetAbsoluteValue(object value)
        {
            base.SetAbsoluteValue(value);
            if (rb != null)
                rb.linearVelocity = Value;
        }

        protected override Vector3 ClampAndProcessValue(Vector3 value)
        {
            // Clamp to max magnitude, then ensure minimum magnitude
            var _val = Vector3.ClampMagnitude(value, MaxValue.magnitude);
            if (_val.magnitude < MinValue.magnitude && _val.sqrMagnitude > 0f)
                _val = _val.normalized * MinValue.magnitude;
            return _val;
        }

        public override void Tick()
        {
            if (rb == null)
                return;
            ensure_max_velocity();
            ensure_min_velocity();

            Value = rb.linearVelocity;

            if (VelocityHistory.Count >= FrameBufferSize)
                VelocityHistory.Dequeue();

            VelocityHistory.Enqueue(rb.linearVelocity);
        }

        // public override IEnumerable<IDebugDrawer> SetupDebugDrawers()
        // {
        //     return new[]
        //     {
        //         new ActionDebugDrawer("Velocities",
        //             (gizmos => gizmos.DrawArrow(transform.position, transform.position + Value, Color.green,0.5F,0.08F)))
        //     };
        // }

        private void ensure_max_velocity()
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MaxValue.magnitude);
        }

        private void ensure_min_velocity()
        {
            if (rb.linearVelocity.sqrMagnitude > 0f && rb.linearVelocity.magnitude < MinValue.magnitude)
                rb.linearVelocity = rb.linearVelocity.normalized * MinValue.magnitude;
        }
    }
}