using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    [RequireComponent(typeof(Rigidbody))]
    public class VelocityBoard3D : Vector3Board
    {
        private Rigidbody Rb;

        [Range(0,100)]
        [SerializeField]
        private int FrameBufferSize = 10;
        
        public readonly Queue<Vector3> VelocityHistory = new Queue<Vector3>();

        [Min(0)]
        [SerializeField]
        private float TopVelocity;

        [Min(0)]
        [SerializeField]
        private float MinVelocity;

        public override Vector3 MinValue => Vector3.one * MinVelocity;
        public override Vector3 MaxValue => Vector3.one * TopVelocity;

        protected override void OnInitializeInternal()
        {
            // Nothing to initialize here
        }

        protected override void OnStart()
        {
            Rb = GetComponent<Rigidbody>();
            InternalValue = Rb.linearVelocity;
            DefaultValue = Rb.linearVelocity;
        }

        protected override Vector3 ClampAndProcessValue(Vector3 value)
        {
            // Clamp to max magnitude, then ensure minimum magnitude
            var val = Vector3.ClampMagnitude(value, MaxValue.magnitude);
            if (val.magnitude < MinValue.magnitude && val.sqrMagnitude > 0f)
                val = val.normalized * MinValue.magnitude;
            return val;
        }

        protected override void OnTick()
        {
            EnsureMaxVelocity();
            EnsureMinVelocity();

            Value = Rb.linearVelocity;

            if (VelocityHistory.Count >= FrameBufferSize)
                VelocityHistory.Dequeue();

            VelocityHistory.Enqueue(Rb.linearVelocity);
        }

        private void EnsureMaxVelocity()
        {
            Rb.linearVelocity = Vector3.ClampMagnitude(Rb.linearVelocity, MaxValue.magnitude);
        }

        private void EnsureMinVelocity()
        {
            if (Rb.linearVelocity.sqrMagnitude > 0f && Rb.linearVelocity.magnitude < MinValue.magnitude)
            {
                Rb.linearVelocity = Rb.linearVelocity.normalized * MinValue.magnitude;
            }
        }
    }
}
