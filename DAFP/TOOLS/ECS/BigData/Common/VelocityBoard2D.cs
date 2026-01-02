using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class VelocityBoard2D : Vector3Board
    {
        private Rigidbody2D Rb;

        [Range(0, 100)] [SerializeField] private int FrameBufferSize = 10;
        public readonly Queue<Vector2> VelocityHistory = new();


        public override void SetAbsoluteValue(object value)
        {
            base.SetAbsoluteValue(value);
            Rb.linearVelocity = Value;
        }

        protected override void OnInitializeInternal()
        {
            if (Rb != null)
                return;
            Rb = Host.GetWorldRepresentation().GetComponent<Rigidbody2D>();
            InternalValue = Rb.linearVelocity;
            DefaultValue = Rb.linearVelocity;
        }


        [Min(0)] [SerializeField] private float TopVelocity;
        [Min(0)] [SerializeField] private float MinVelocity;
        public override Vector3 MinValue => Vector3.one * MinVelocity;

        protected override Vector3 ClampAndProcessValue(Vector3 value)
        {
            var val = Vector3.ClampMagnitude(value, MaxValue.magnitude);
            val = val.ClampMinMagnitude(MinValue.magnitude);

            return val;
        }

        public override Vector3 MaxValue => Vector3.one * TopVelocity;

        public override void Tick()
        {
            if (Rb != null)
                return;
            ensure_max_velocity();
            ensure_min_velocity();
            Value = Rb.linearVelocity;

            if (VelocityHistory.Count >= FrameBufferSize)
                VelocityHistory.Dequeue(); // remove the oldest velocity

            VelocityHistory.Enqueue(Rb.linearVelocity);
        }

        private void ensure_max_velocity()
        {
            Rb.linearVelocity = Vector2.ClampMagnitude(Rb.linearVelocity, MaxValue.magnitude);
        }

        // public override IEnumerable<IDebugDrawer> SetupDebugDrawers()
        // {
        //     return new[]
        //     {
        //         new ActionDebugDrawer("Velocities",
        //             (gizmos => gizmos.DrawArrow(transform.position, transform.position + Value, Color.green, 0.5F,
        //                 0.08F)))
        //     };
        // }

        private void ensure_min_velocity()
        {
            if (Rb.linearVelocity.magnitude < MinValue.magnitude)
                Rb.linearVelocity = Rb.linearVelocity.normalized * MinValue.magnitude;
        }
    }
}