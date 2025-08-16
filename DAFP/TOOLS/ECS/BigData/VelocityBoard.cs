using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.ECS.BigData
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class VelocityBoard : Vector3Board
    {
        private Rigidbody2D Rb;

        [Range(0,100)][SerializeField] private int FrameBufferSize = 10;
        public readonly Queue<Vector2> VelocityHistory = new Queue<Vector2>();

        protected override void OnStart()
        {
            Rb = GetComponent<Rigidbody2D>();
            InternalValue = Rb.linearVelocity;
            DefaultValue = Rb.linearVelocity;
        }

        protected override void OnInitializeInternal()
        {
        }


        [Min(0)][SerializeField] private float TopVelocity;
        [Min(0)][SerializeField] private float MinVelocity;
        public override Vector3 MinValue => Vector3.one * MinVelocity;
        protected override Vector3 ClampAndProcessValue(Vector3 value)
        {
            
            var val = Vector3.ClampMagnitude(value, MaxValue.magnitude);
            val = val.ClampMinMagnitude(MinValue.magnitude);

            return val;
        }

        public override Vector3 MaxValue => Vector3.one * TopVelocity;

        protected override void OnTick()
        {
            EnsureMaxVelocity();
            EnsureMinVelocity();
            Value = Rb.linearVelocity;

            if (VelocityHistory.Count >= FrameBufferSize)
                VelocityHistory.Dequeue(); // remove the oldest velocity

            VelocityHistory.Enqueue(Rb.linearVelocity);
        }

        void EnsureMaxVelocity()
        {
            Rb.linearVelocity = Vector2.ClampMagnitude(Rb.linearVelocity, MaxValue.magnitude);
        }

        void EnsureMinVelocity()
        {
            if (Rb.linearVelocity.magnitude < MinValue.magnitude)
            {
                Rb.linearVelocity = Rb.linearVelocity.normalized * MinValue.magnitude;
            }
        }
    }
}