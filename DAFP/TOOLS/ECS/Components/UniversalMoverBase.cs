using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Services;
using PixelRouge.Colors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.Components
{
    using DAFP.TOOLS.Common.Maths;

    public abstract class UniversalMoverBase<TVec, TRb, TMode> : EntityComponent, IUniversalMover
        where TRb : Component
    {
        protected TRb Rb;
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

        // Explicit interface properties to expose fields without changing existing API
        bool IUniversalMover.CanFly
        {
            get => CanFly;
            set => CanFly = value;
        }

        float IUniversalMover.MaxFallSpeed
        {
            get => MaxFallSpeed;
            set => MaxFallSpeed = value;
        }

        private Queue<MovementCommand> commandQueue = new();

        private int resetTimer;

        [Inject(Id = "DefaultPhysicsComponentGameplayTicker")]
        public override ITickerBase EntityComponentTicker { get; }

        [RequireStat] [InjectStat("Acceleration")]
        private IStat<float> accelerationBoard;

        [InjectStat("Deceleration")] [RequireStat]
        private IStat<float> decelerationBoard;

        [InjectStat("MovementSpeed")] [RequireStat]
        private IStat<float> movementSpeedBoard;

        [InjectStat("Stunned")] [RequireStat] private IStat<bool> isStunnedBoard;

        [InjectStat("CanMove")] [RequireStat] private IStat<bool> canMoveBoard;

        protected override void OnInitialize()
        {
            Rb = GetComponent<TRb>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
            handle_input_movement(InputMovement);
            clamp_max_fall_speed();
            handle_input_resets();


#if UNITY_EDITOR
            CommandQueueCount = commandQueue.Count;
#endif
            execute_movement_commands();
        }

        private void handle_input_resets()
        {
            resetTimer++;
            UpdatesPerInputSend++;
            if (resetTimer >= MaxInputResetWaitTime)
            {
                reset_input();
                reset_input_timer();
            }
        }

        private void reset_input()
        {
            InputMovement = ZeroVector;
        }

        private void execute_movement_commands()
        {
            while (commandQueue.Count > 0) commandQueue.Dequeue().Func.Invoke();
        }

        // IUniversalMover explicit implementations using IVectorBase for vector-based abilities
        void IUniversalMover.DoDash(IVectorBase force, float time) => DoDash(FromIVector(force), time);
        void IUniversalMover.DoJump(IVectorBase jump) => DoJump(FromIVector(jump));
        void IUniversalMover.DoCutJump(IVectorBase positive, float m) => DoCutJump(FromIVector(positive), m);

        void IUniversalMover.AddKnockback(IVectorBase force, float time, float delay) =>
            AddKnockback(FromIVector(force), time, delay);

        private void add_command(MovementCommand command)
        {
            if (commandQueue.Any(movementCommand => command.Name == movementCommand.Name))
                return;
            commandQueue.Enqueue(command);
        }

        private void reset_input_timer()
        {
            resetTimer = 0;
        }

        public void Input(TVec input)
        {
            InputMovement = input;
            reset_input_timer();
            MaxInputResetWaitTime = 3 * Mathf.Clamp(UpdatesPerInputSend, 1, 100);
            UpdatesPerInputSend = 0;
        }

        // IUniversalMover explicit implementations using IVectorBase
        void IUniversalMover.Input(DAFP.TOOLS.Common.Maths.IVectorBase input)
        {
            Input(FromIVector(input));
        }

        public void DoDash(TVec force, float time)
        {
            if (!canMoveBoard.Value) return;
            if (isStunnedBoard.Value) return;
            if (IsInDash)
                return;
            IsInDash = true;
            StartCoroutine(dash_coroutine(force, time));
        }

        public void DoJump(TVec jump)
        {
            add_command(new MovementCommand(() => { AddForce(jump, DefaultForceMode()); }, nameof(DoJump)));
        }

        public void DoHalt(float divisor)
        {
            divisor = Mathf.Max(divisor, 1);
            add_command(new MovementCommand(() =>
                SetVelocity(Divide(Velocity, divisor)), nameof(DoHalt)));
        }

        public void DoCutJump(TVec positive, float m)
        {
            add_command(new MovementCommand(() =>
            {
                if (Angle(positive, Velocity) < 50f)
                    SetVelocity(Divide(Velocity, m));
            }, nameof(DoCutJump)));
        }

        public void AddKnockback(TVec force, float time, float delay)
        {
            if (IsInKnockback) return;
            StartCoroutine(knockback_coroutine(force, time, delay));
        }

// #if UNITY_EDITOR
//         private void OnDrawGizmos()
//         {
//             Gizmos.color = Color.white;
//             // cast TVec to Vector3 for gizmos
//             if (InputMovement is Vector2 v2)
//                 Gizmos.DrawLine(transform.position, transform.position + (Vector3)v2);
//             else if (InputMovement is Vector3 v3)
//                 Gizmos.DrawLine(transform.position, transform.position + v3);
//         }
// #endif

        // -- SHARED MOVEMENT LOGIC --
        private TVec wish_velocity;

        private void handle_input_movement(TVec inputMovement)
        {
            if (!canMoveBoard.Value) return;
            if (isStunnedBoard.Value) return;


            var _accel = accelerationBoard.Value;
            var _decel = decelerationBoard.Value;
            var _wishVel = Multiply(inputMovement, movementSpeedBoard.Value);
            wish_velocity = _wishVel;
            var _curVel = Velocity;
            // only apply XY or XYZ depending on CanFly
            var _raw = _wishVel;
            if (!CanFly)
                _raw = MaskOutAxis(_raw, 1); // axis 1 = Y
            AddForce(_raw, DefaultForceMode());

            var _dt = EntityComponentTicker.DeltaTime;
            // build a force vector step by step
            var _force = ZeroVector;

            for (var _axis = 0; _axis < Dimension; _axis++)
            {
                var _wish = GetComponent(_wishVel, _axis);
                var _cur = GetComponent(_curVel, _axis);

                // skip vertical if !CanFly
                if (_axis == 1 && !CanFly)
                    continue;

                var _delta = _wish - _cur;
                var _target = Mathf.Abs(_wish);
                var _current = Mathf.Abs(_cur);
                var _speed = _target > _current ? _accel : _decel;
                var _threshold = _speed * _dt / GetMass();
#if UNITY_EDITOR
                MovementPrecision = SetComponent(MovementPrecision, _axis, _threshold);
#endif
                if (Mathf.Abs(_delta) > _threshold)
                {
                    var _f = Mathf.Sign(_delta) * _speed;
                    _force = SetComponent(_force, _axis, _f);
                }
            }

            AddForce(_force, DefaultForceMode());
        }

        // private void GroundAccel(TVec inputMovement) Deprecated
        // {
        //     if (movementSpeedBoard.Value == 0)
        //         return;
        //     var wishvel = Multiply(inputMovement, movementSpeedBoard.Value);
        //     var pushvec = Subtract(wishvel, Velocity);
        //     var addspeed = Magnitude(pushvec);
        //     var accelspeed = accelerationBoard.Value * EntityComponentTicker.DeltaTime * addspeed;
        //     pushvec = Normalize(pushvec);
        //
        //     if (accelspeed > addspeed)
        //         accelspeed = addspeed;
        //     AddForce(Multiply(pushvec,accelspeed));
        // }
        //
        // private void AirAccel(TVec inputMovement)
        // {
        //     var wish_velocity = Normalize(inputMovement);
        //     float wish_speed = movementSpeedBoard.Value;
        //
        //     if (wish_speed > 30)
        //         wish_speed = 30;
        //     var current_speed = DotProduct(Velocity, wish_velocity);
        //
        //     float add_speed = wish_speed - current_speed;
        //     if (add_speed <= 0)
        //         return;
        //     var accel_speed = movementSpeedBoard.Value * accelerationBoard.Value * EntityComponentTicker.DeltaTime;
        //     if (accel_speed > add_speed)
        //         accel_speed = add_speed;
        //     AddForce(Multiply(wish_velocity, accel_speed));
        // }

        private void clamp_max_fall_speed()
        {
            // only affect downward Y
            var _y = GetComponent(Velocity, 1);
            if (_y < -MaxFallSpeed) Velocity = SetComponent(Velocity, 1, -MaxFallSpeed);
        }

        private IEnumerator dash_coroutine(TVec force, float time)
        {
            var _modMove = new LockModifier<bool>(Host, false, -50);
            var _modStun = new LockModifier<bool>(Host, true, -50);
            canMoveBoard.AddModifier(_modMove);
            isStunnedBoard.AddModifier(_modStun);

            // impulse
            commandQueue.Enqueue(new MovementCommand(() =>
                AddForce(Multiply(force, movementSpeedBoard.Value), ImpulseMode()), "DashAddForce"));
            yield return new WaitForSeconds(time);

            IsInDash = false;
            DoHalt(Magnitude(force) / 2f);

            canMoveBoard.RemoveModifier(_modMove);
            isStunnedBoard.RemoveModifier(_modStun);
        }

        private IEnumerator knockback_coroutine(TVec force, float time, float delay)
        {
            var _modMove = new LockModifier<bool>(Host, false, -40);
            var _modStun = new LockModifier<bool>(Host, true, -40);
            canMoveBoard.AddModifier(_modMove);
            isStunnedBoard.AddModifier(_modStun);

            yield return new WaitForSeconds(delay);

            IsInKnockback = true;
            commandQueue.Enqueue(new MovementCommand(() =>
                AddForce(Multiply(force, movementSpeedBoard.Value), ImpulseMode()), "KnockbackAddForce"));
            yield return new WaitForSeconds(time);
            IsInKnockback = false;

            canMoveBoard.RemoveModifier(_modMove);
            isStunnedBoard.RemoveModifier(_modStun);
        }

        public override IEnumerable<IDebugDrawer> SetupDebugDrawers()
        {
            return new[]
            {
                new ActionDebugDrawer("Velocities", (gizmos => gizmos.DrawArrow(transform.position,
                    GetVec3(Add(Normalize(wish_velocity), CurrentPos())), ColorsForUnity.Orangered)))
            };
        }
        // --- ABSTRACT / HELPERS TO IMPLEMENT IN SUBCLASS --

        protected abstract TVec Normalize(TVec vec);
        protected abstract float DotProduct(TVec vec1, TVec vec2);
        protected abstract float GetMass();
        protected abstract TVec Velocity { get; set; }
        protected abstract void SetVelocity(TVec v);
        public abstract void AddForce(TVec force, TMode mode);
        protected abstract void AddForce(TVec force); // default ForceMode if any
        protected abstract TMode DefaultForceMode(); // e.g. ForceMode.Force or ForceMode2D.Force
        protected abstract TMode ImpulseMode(); // e.g. ForceMode.Impulse or ForceMode2D.Impulse

        /// <summary> Create a zero vector (Vector2.zero or Vector3.zero) </summary>
        protected abstract TVec ZeroVector { get; }

        /// <summary> Number of axes: 2 or 3 </summary>
        protected abstract int Dimension { get; }

        protected abstract TVec Multiply(TVec v, float scalar);
        protected abstract TVec Subtract(TVec v, TVec v1);

        protected abstract TVec Add(TVec v, TVec v1);
        protected abstract TVec CurrentPos();
        protected abstract TVec Divide(TVec v, float scalar);
        protected abstract float GetComponent(TVec v, int axis);
        protected abstract TVec SetComponent(TVec v, int axis, float value);
        protected abstract TVec MaskOutAxis(TVec v, int axis);
        protected abstract Vector3 GetVec3(TVec v);
        protected abstract float Magnitude(TVec v);
        protected abstract float Angle(TVec from, TVec to);

        // Convert from common vector interface to concrete TVec (Vector2/Vector3)
        protected abstract TVec FromIVector(IVectorBase v);
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