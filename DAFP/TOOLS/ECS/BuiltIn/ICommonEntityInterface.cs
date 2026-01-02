using System;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.Filters;
using Optional;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public interface ICommonEntityInterface : IEntity
    {
        public interface IVelocityProvider : ICommonEntityInterface
        {
            public IVectorBase Velocity { get; }
        }

        public interface IEntMeleeAttackInputable : ICommonEntityInterface
        {
            public void InputMeleeAttack(IVectorBase vector);
            public void InputMeleeAttack();
        }

        public interface IEntMovementInputable : ICommonEntityInterface
        {
            public void InputMovement(IVectorBase vector);
        }

        public interface IEntJumpInputable : ICommonEntityInterface
        {
            public void InputMovement(float force);
        }

        public interface IEntPathFindable : IEntTargetContainable
        {
            public IVectorBase PathFindToTarget();
        }

        public interface IEntTargetContainable : ICommonEntityInterface
        {
            public Option<IEntity> Target { get; set; }
            public void ResetTarget() => Target = Option.None<IEntity>();
        }

        public interface IEntTargetDetectable : IEntTargetContainable
        {
            public IEntity ScanForTarget(IFilter<IEntity> filter);
        }

        public interface IDieable : IDamageable
        {
            public bool Alive => !Dead;
            public bool Dead { get; }
            public event Action<ICommonEntityEvent.EntityDieEvent> OnDie;

            public void Die(IDamage lethal);
        }
    }

    public interface ICommonEntityEvent
    {
        public IEntity Host { get; }

        // public struct EntityTriggerMeleeAttackEvent : ICommonEntityEvent
        // {
        //     public int Variation { get; init; }
        //     public IVectorBase Direction { get; init; }
        //     public IEntity Host { get; init; }
        // }
        public struct EntityDieEvent : ICommonEntityEvent
        {
            public EntityDieEvent(IEntity host, IDamage lethal)
            {
                Host = host;
                Lethal = lethal;
            }

            public IEntity Host { get;  }
            public IDamage Lethal { get;  }
        }

        public struct EntityTakeDamageEvent : ICommonEntityEvent
        {
            public EntityTakeDamageEvent(IEntity host, IDamage damage)
            {
                Host = host;
                Damage = damage;
            }

            public IEntity Host { get;  }
            public IDamage Damage { get;  }
        }
    }
}