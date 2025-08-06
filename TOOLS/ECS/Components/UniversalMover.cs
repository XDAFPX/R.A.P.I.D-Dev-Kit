using System.Collections;
using DAFP.GAME.Essential;
using UnityEngine;

namespace DAFP.Game.Utill
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class UniversalMover :MonoBehaviour
    {
        private Rigidbody2D rb;
        public bool CanMove;

        public bool CanFly;
        public float Speed;
        public bool IsStunned {  get { return IsInDash || IsInKnockback; } }
        public bool IsInKnockback { get; private set; }
        public bool IsInDash { get; private set; }
        [SerializeField] private Vector2 InputMovement;

        public float AccelerationSpeed = 1;
        public float DecelerationSpeed = 1;
        public Rigidbody2D GetRb()
        {
            return rb;
        }
        private void Awake()
        {
            Initialize();
        }
        private void Initialize()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        public void Input(Vector2 input) { InputMovement = input; }
        private void FixedUpdate()
        {
            HandleInputMovement(InputMovement);
            ResetInput();
        }
        private void ResetInput()
        {
            InputMovement = Vector2.zero;   
        }
        private void HandleInputMovement(Vector2 inputMovement)
        {
            if (!CanMove) return;
            if(IsStunned) return;
            Vector2 _wishdir = this.InputMovement * Speed;


            Vector2 _velocity = rb.linearVelocity;

            // Horizontal (X)
            float _deltaX = _wishdir.x - _velocity.x;
            float _forceX = 0;
            if (Mathf.Abs(_deltaX) > 0.01f)
            {
                float _accel = _deltaX > 0 ? AccelerationSpeed : DecelerationSpeed;
                _forceX = Mathf.Sign(_deltaX) * _accel;
            }
            float _deltaY = _wishdir.y - _velocity.y;
            float _forceY = 0;
            if (CanFly)
            {
                // Vertical (Y)

                if (Mathf.Abs(_deltaY) > 0.01f)
                {
                    float _accel = _deltaY > 0 ? AccelerationSpeed : DecelerationSpeed;
                    _forceY = Mathf.Sign(_deltaY) * _accel;
                }
            }


            rb.AddForce(new Vector2(_forceX, _forceY));
            


        }

        public void DoDash(Vector2 force, float time)
        {
            if (!CanMove)
                return;
            if (IsStunned)
                return;
            IsInDash = true;
            StartCoroutine(Dash(force, time));

        }
        private IEnumerator Dash(Vector2 force, float time)
        {
            IsInDash = true;
            rb.AddForce(force*Speed,ForceMode2D.Impulse);
            yield return new WaitForSeconds(time);
            IsInDash = false;
            Halt(force.magnitude/2);
        }
        public void Halt(float force)
        {
            rb.linearVelocity /= force;
        }
        public void AddKnockback(Vector2 force, float time, float delay)
        {
            if (IsInKnockback)
                return;
            StartCoroutine(Knockback(force, time, delay));
        }

        private IEnumerator Knockback(Vector2 force, float time, float delay)
        {
            yield return new WaitForSeconds(delay);
            IsInKnockback = true;
            rb.AddForce(force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(time);
            IsInKnockback = false;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position.Add(InputMovement));
        }


#endif
    
        

    }
}
