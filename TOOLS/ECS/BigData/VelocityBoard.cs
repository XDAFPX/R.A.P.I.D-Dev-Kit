using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class VelocityBoard : Vector3Board
    {
        private Rigidbody2D Rb;
        protected override void OnInitialize()
        {
            
        }

        protected override void OnStart()
        {
            Rb = GetComponent<Rigidbody2D>();
            InternalValue = Rb.linearVelocity;
            DefaultValue = Rb.linearVelocity;
        }

        protected override void OnTick()
        {
            EnsureMaxVelocity();
            EnsureMinVelocity();
            SetValue(Rb.linearVelocity);
        }

        void EnsureMaxVelocity() {
            Rb.linearVelocity = Vector2.ClampMagnitude(Rb.linearVelocity,MaxValue.magnitude);
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