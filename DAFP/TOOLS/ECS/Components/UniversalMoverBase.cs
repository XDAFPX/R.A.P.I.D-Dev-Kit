using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    public abstract class UniversalMoverBase<TVec, TRb, TMode> : EntityComponent
        where TRb : Component
    {
        protected TRb rb;
        [ReadOnly] [SerializeField] protected TVec InputMovement;


#if UNITY_EDITOR
        [ReadOnly] [SerializeField] protected TVec MovementPrecision;
        [ReadOnly] [SerializeField] protected int CommandQueueCount;
#endif
        [FormerlySerializedAs("maxInputResetWaitTime")]
        [ReadOnly]
        [Tooltip("Every ~1 of this time = 0.02s ")]
        [SerializeField]
        private int MaxInputResetWaitTime = 15;

        [ReadOnly] [SerializeField] private int UpdatesPerInputSend = 1;
        public bool CanFly;
        public bool IsInKnockback { get; protected set; }
        public bool IsInDash { get; protected set; }
        public float MaxFallSpeed = 50f;
        private Queue<MovementCommand> commandQueue = new();

        private int resetTimer;
        public override ITickerBase EntityComponentTicker => World.ComponentFixedUpdateTicker;

        [GetComponentCache] private AccelerationBoard accelerationBoard;
        [GetComponentCache] private DecelerationBoard decelerationBoard;
        [GetComponentCache] private MovementSpeedBoard movementSpeedBoard;
        [GetComponentCache] private IsStunnedBoard isStunnedBoard;
        [GetComponentCache] private CanMoveBoard canMoveBoard;

        protected override void OnInitialize()
        {
            rb = GetComponent<TRb>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
            HandleInputMovement(InputMovement);
            ClampMaxFallSpeed();
            HandleInputResets();


#if UNITY_EDITOR
            CommandQueueCount = commandQueue.Count;
#endif
            ExecuteMovementCommands();
        }

        private void HandleInputResets()
        {
            resetTimer++;
            UpdatesPerInputSend++;
            if (resetTimer >= MaxInputResetWaitTime)
            {
                ResetInput();
                ResetInputTimer();
            }
        }

        private void ResetInput()
        {
            InputMovement = ZeroVector;
        }

        private void ExecuteMovementCommands()
        {
            while (commandQueue.Count > 0)
            {
                commandQueue.Dequeue().Func.Invoke();
            }
        }

        private void AddCommand(MovementCommand command)
        {
            if (commandQueue.Any((movementCommand => command.Name == movementCommand.Name)))
                return;
            commandQueue.Enqueue(command);
        }

        private void ResetInputTimer()
        {
            resetTimer = 0;
        }

        public void Input(TVec input)
        {
            InputMovement = input;
            ResetInputTimer();
            MaxInputResetWaitTime = 3 * Mathf.Clamp(UpdatesPerInputSend, 1, 100);
            UpdatesPerInputSend = 0;
        }

        public void DoDash(TVec force, float time)
        {
            if (!canMoveBoard.Value) return;
            if (isStunnedBoard.Value) return;
            if (IsInDash)
                return;
            IsInDash = true;
            StartCoroutine(DashCoroutine(force, time));
        }

        public void DoJump(TVec jump)
        {
            AddCommand(new MovementCommand((() => { AddForce(jump, DefaultForceMode()); }), nameof(DoJump)));
        }

        public void DoHalt(float divisor)
        {
            AddCommand(new MovementCommand((() =>
                SetVelocity(Divide(Velocity, divisor))), nameof(DoHalt)));
        }

        public void DoCutJump(TVec positive, float m)
        {
            AddCommand(new MovementCommand((() =>
            {
                if (Angle(positive, Velocity) < 50f)
                    SetVelocity(Divide(Velocity, m));
            }), nameof(DoCutJump)));
        }

        public void AddKnockback(TVec force, float time, float delay)
        {
            if (IsInKnockback) return;
            StartCoroutine(KnockbackCoroutine(force, time, delay));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            // cast TVec to Vector3 for gizmos
            if (InputMovement is Vector2 v2)
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)v2);
            else if (InputMovement is Vector3 v3)
                Gizmos.DrawLine(transform.position, transform.position + v3);
        }
#endif

        // -- SHARED MOVEMENT LOGIC --
        private void HandleInputMovement(TVec inputMovement)
        {
            if (!canMoveBoard.Value) return;
            if (isStunnedBoard.Value) return;

            float accel = accelerationBoard.Value;
            float decel = decelerationBoard.Value;
            TVec wishVel = Multiply(inputMovement, movementSpeedBoard.Value);
            TVec curVel = Velocity;

            // only apply XY or XYZ depending on CanFly
            TVec raw = wishVel;
            if (!CanFly)
                raw = MaskOutAxis(raw, 1); // axis 1 = Y
            AddForce(raw, DefaultForceMode());

            float dt = EntityComponentTicker.DeltaTime;
            // build a force vector step by step
            TVec force = ZeroVector;

            for (int axis = 0; axis < Dimension; axis++)
            {
                float wish = GetComponent(wishVel, axis);
                float cur = GetComponent(curVel, axis);

                // skip vertical if !CanFly
                if (axis == 1 && !CanFly)
                    continue;

                float delta = wish - cur;
                float target = Mathf.Abs(wish);
                float current = Mathf.Abs(cur);
                float speed = target > current ? accel : decel;
                float threshold = speed * dt;
#if UNITY_EDITOR
                MovementPrecision = SetComponent(MovementPrecision, axis, threshold);
#endif
                if (Mathf.Abs(delta) > threshold)
                {
                    float f = Mathf.Sign(delta) * speed;
                    force = SetComponent(force, axis, f);
                }
            }

            AddForce(force, DefaultForceMode());
        }

        private void ClampMaxFallSpeed()
        {
            // only affect downward Y
            float y = GetComponent(Velocity, 1);
            if (y < -MaxFallSpeed)
            {
                Velocity = SetComponent(Velocity, 1, -MaxFallSpeed);
            }
        }

        private IEnumerator DashCoroutine(TVec force, float time)
        {
            var modMove = new LockModifier<bool>(Host, false, -50);
            var modStun = new LockModifier<bool>(Host, true, -50);
            canMoveBoard.AddModifier(modMove);
            isStunnedBoard.AddModifier(modStun);

            // impulse
            commandQueue.Enqueue(new MovementCommand((() =>
                AddForce(Multiply(force, movementSpeedBoard.Value), ImpulseMode())), "DashAddForce"));
            yield return new WaitForSeconds(time);

            IsInDash = false;
            DoHalt(Magnitude(force) / 2f);

            canMoveBoard.RemoveModifier(modMove);
            isStunnedBoard.RemoveModifier(modStun);
        }

        private IEnumerator KnockbackCoroutine(TVec force, float time, float delay)
        {
            var modMove = new LockModifier<bool>(Host, false, -40);
            var modStun = new LockModifier<bool>(Host, true, -40);
            canMoveBoard.AddModifier(modMove);
            isStunnedBoard.AddModifier(modStun);

            yield return new WaitForSeconds(delay);

            IsInKnockback = true;
            commandQueue.Enqueue(new MovementCommand((() =>
                AddForce(Multiply(force, movementSpeedBoard.Value), ImpulseMode())), "KnockbackAddForce"));
            yield return new WaitForSeconds(time);
            IsInKnockback = false;

            canMoveBoard.RemoveModifier(modMove);
            isStunnedBoard.RemoveModifier(modStun);
        }


        // --- ABSTRACT / HELPERS TO IMPLEMENT IN SUBCLASS --
        protected abstract TVec Velocity { get; set; }
        protected abstract void SetVelocity(TVec v);
        protected abstract void AddForce(TVec force, TMode mode);
        protected abstract void AddForce(TVec force); // default ForceMode if any
        protected abstract TMode DefaultForceMode(); // e.g. ForceMode.Force or ForceMode2D.Force
        protected abstract TMode ImpulseMode(); // e.g. ForceMode.Impulse or ForceMode2D.Impulse

        /// <summary> Create a zero vector (Vector2.zero or Vector3.zero) </summary>
        protected abstract TVec ZeroVector { get; }

        /// <summary> Number of axes: 2 or 3 </summary>
        protected abstract int Dimension { get; }

        protected abstract TVec Multiply(TVec v, float scalar);
        protected abstract TVec Divide(TVec v, float scalar);
        protected abstract float GetComponent(TVec v, int axis);
        protected abstract TVec SetComponent(TVec v, int axis, float value);
        protected abstract TVec MaskOutAxis(TVec v, int axis);
        protected abstract float Magnitude(TVec v);
        protected abstract float Angle(TVec from, TVec to);
    }

    internal struct MovementCommand : INameable
    {
        public readonly Action Func;

        public MovementCommand(Action func, string name)
        {
            Func = func;
            Name = name;
        }

        public string Name { get; set; }
    }
}