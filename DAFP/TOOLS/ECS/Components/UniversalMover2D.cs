using System.Collections;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CanMoveBoard))]
    [RequireComponent(typeof(IsStunnedBoard))]
    [RequireComponent(typeof(MovementSpeedBoard))]
    public class UniversalMover2D : EntityComponent
    {
        private Rigidbody2D rb;

        [ReadOnly] [SerializeField] private Vector2 InputMovement;
#if UNITY_EDITOR
        [ReadOnly] [SerializeField] private Vector2 MovementPrecision;
#endif
        public bool CanFly;
        public bool CapSpeed = true;
        public bool IsInKnockback { get; private set; }
        public bool IsInDash { get; private set; }

        protected override void OnInitialize()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
            HandleInputMovement(InputMovement);
            InputMovement = Vector2.zero;
        }

        public void Input(Vector2 input)
        {
            InputMovement = input;
        }

        private void HandleInputMovement(Vector2 inputMovement)
        {
            if (!Host.GetEntComponent<CanMoveBoard>().Value) return;
            if (Host.GetEntComponent<IsStunnedBoard>().Value) return;

            float AccelerationSpeed = GetEntComponent<AccelerationBoard>().Value;
            float DecelerationSpeed = GetEntComponent<DecelerationBoard>().Value;
            Vector2 wishVel = inputMovement * Host.GetEntComponent<MovementSpeedBoard>().Value;
            Vector2 curVel = rb.linearVelocity;

            if (!CapSpeed)
            {
                rb.AddForce(new Vector2(wishVel.x, CanFly ? wishVel.y : 0));
                return;
            }

            float dt = Host.EntityTicker.DeltaTime;

            // Horizontal (X)
            float deltaX = wishVel.x - curVel.x;
            float forceX = 0f;
            {
                float targetX = Mathf.Abs(wishVel.x);
                float currentX = Mathf.Abs(curVel.x);
                float accelX = targetX > currentX ? AccelerationSpeed : DecelerationSpeed;
                float thresholdX = accelX * dt;
#if UNITY_EDITOR
                MovementPrecision.x = thresholdX;
#endif
                if (Mathf.Abs(deltaX) > thresholdX)
                    forceX = Mathf.Sign(deltaX) * accelX;
            }

            // Vertical (Y)
            float forceY = 0f;
            if (CanFly)
            {
                float deltaY = wishVel.y - curVel.y;
                float targetY = Mathf.Abs(wishVel.y);
                float currentY = Mathf.Abs(curVel.y);
                float accelY = targetY > currentY ? AccelerationSpeed : DecelerationSpeed;
                float thresholdY = accelY * dt;
#if UNITY_EDITOR
                MovementPrecision.y = thresholdY;
#endif
                if (Mathf.Abs(deltaY) > thresholdY)
                    forceY = Mathf.Sign(deltaY) * accelY;
            }

            rb.AddForce(new Vector2(forceX, forceY));
        }

        public void DoDash(Vector2 force, float time)
        {
            if (!Host.GetEntComponent<CanMoveBoard>().Value) return;
            if (Host.GetEntComponent<IsStunnedBoard>().Value) return;
            IsInDash = true;
            StartCoroutine(Dash(force, time));
        }

        public void Jump(Vector2 jump)
        {
            rb.AddForce(jump, ForceMode2D.Force);
        }

        private IEnumerator Dash(Vector2 force, float time)
        {
            var modMove = new LockModifier<bool>(Host, false, -50);
            var modStun = new LockModifier<bool>(Host, true, -50);
            GetEntComponent<CanMoveBoard>().AddModifier(modMove);
            GetEntComponent<IsStunnedBoard>().AddModifier(modStun);

            rb.AddForce(force * GetEntComponent<MovementSpeedBoard>().Value, ForceMode2D.Impulse);
            yield return new WaitForSeconds(time);

            IsInDash = false;
            Halt(force.magnitude / 2f);

            GetEntComponent<CanMoveBoard>().RemoveModifier(modMove);
            GetEntComponent<IsStunnedBoard>().RemoveModifier(modStun);
        }

        public void Halt(float divisor)
        {
            rb.linearVelocity /= divisor;
        }

        public void AddKnockback(Vector2 force, float time, float delay)
        {
            if (IsInKnockback) return;
            StartCoroutine(Knockback(force, time, delay));
        }

        private IEnumerator Knockback(Vector2 force, float time, float delay)
        {
            var modMove = new LockModifier<bool>(Host, false, -40);
            var modStun = new LockModifier<bool>(Host, true, -40);
            GetEntComponent<CanMoveBoard>().AddModifier(modMove);
            GetEntComponent<IsStunnedBoard>().AddModifier(modStun);

            yield return new WaitForSeconds(delay);

            IsInKnockback = true;
            rb.AddForce(force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(time);
            IsInKnockback = false;

            GetEntComponent<CanMoveBoard>().RemoveModifier(modMove);
            GetEntComponent<IsStunnedBoard>().RemoveModifier(modStun);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)InputMovement);
        }
#endif
    }
}