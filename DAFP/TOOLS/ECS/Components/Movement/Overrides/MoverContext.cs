using System;
using DAFP.TOOLS.Common.Maths;

namespace RapidLib.DAFP.TOOLS.ECS.Components.Movement.Overrides
{
    /// <summary>
    /// Read-only view of mover state passed into every override hook.
    /// Keeps overrides decoupled from the concrete MoverBase type.
    /// </summary>
    public enum OverrideResult
    {
        /// <summary> Run default behavior after this override. </summary>
        Continue,

        /// <summary> Skip default behavior but keep calling remaining overrides. </summary>
        Suppress,
    }

    public interface IMoverActions
    {
        void IntegrateForce(IVector force, bool impulse = false);
        void SetVelocity(IVector velocity);
        void EnqueueCommand(string name, Action command);
    }

    public readonly struct MoverContext
    {
        public MoverContext(bool canFly, bool isInKnockback, bool isInDash, float deltaTime, float mass,
            float acceleration, float deceleration, float movementSpeed, bool isStunned, bool canMove, IVector velocity,
            IVector position, IMoverActions actions)
        {
            this.Actions = actions;
            CanFly = canFly;
            IsInKnockback = isInKnockback;
            IsInDash = isInDash;
            DeltaTime = deltaTime;
            Mass = mass;
            Acceleration = acceleration;
            Deceleration = deceleration;
            MovementSpeed = movementSpeed;
            IsStunned = isStunned;
            CanMove = canMove;
            Velocity = velocity;
            Position = position;
        }

        public bool CanFly { get; }
        public bool IsInKnockback { get; }
        public bool IsInDash { get; }
        public float DeltaTime { get; }
        public float Mass { get; }

        public float Acceleration { get; }
        public float Deceleration { get; }
        public float MovementSpeed { get; }
        public bool IsStunned { get; }
        public bool CanMove { get; }

        public IVector Velocity { get; }
        public IVector Position { get; }

        public IMoverActions Actions { get; }
    }
}