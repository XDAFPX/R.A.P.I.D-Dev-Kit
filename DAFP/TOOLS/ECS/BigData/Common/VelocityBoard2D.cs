using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    public class VelocityBoard2D : Vector3Board, ITickable, ICommonEntityInterface.IVelocityProvider.IVelocityStat,
        ICommonEntityInterface.IMovementProvider.IMovingStat
    {
        private Rigidbody2D rb;

        [Range(0, 100)] [SerializeField] private int frameBufferSize = 10;
        [Range(0, 300)] [SerializeField] private float isMovingTreshold = 1;
        public readonly Queue<Vector2> VelocityHistory = new();


        public override void SetAbsoluteValue(object value)
        {
            base.SetAbsoluteValue(value);
            rb.linearVelocity = Value;
        }

        protected override void OnInitializeInternal()
        {
            if (rb != null)
                return;
            rb = Host.GetWorldRepresentation().GetComponent<Rigidbody2D>();
            InternalValue = rb.linearVelocity;
            DefaultValue = rb.linearVelocity;
        }


        [Min(0)] [SerializeField] private float topVelocity = 1000;
        [Min(0)] [SerializeField] private float minVelocity;
        public override Vector3 MinValue => Vector3.one * minVelocity;

        protected override Vector3 ClampAndProcessValue(Vector3 value)
        {
            var _val = Vector3.ClampMagnitude(value, MaxValue.magnitude);
            _val = _val.ClampMinMagnitude(MinValue.magnitude);

            return _val;
        }

        public override Vector3 MaxValue => Vector3.one * topVelocity;

        public override void Tick()
        {
            if (rb == null)
                return;
            ensure_max_velocity();
            ensure_min_velocity();
            Value = rb.linearVelocity;

            if (VelocityHistory.Count >= frameBufferSize)
                VelocityHistory.Dequeue(); // remove the oldest velocity

            VelocityHistory.Enqueue(rb.linearVelocity);
        }

        private void ensure_max_velocity()
        {
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, MaxValue.magnitude);
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
            if (rb.linearVelocity.magnitude < MinValue.magnitude)
                rb.linearVelocity = rb.linearVelocity.normalized * MinValue.magnitude;
        }

        IVectorBase ICommonEntityInterface.IVelocityProvider.IVelocityStat.Value => (V3)Value;
        public bool IsMoving => Value.sqrMagnitude > isMovingTreshold;
    }
}