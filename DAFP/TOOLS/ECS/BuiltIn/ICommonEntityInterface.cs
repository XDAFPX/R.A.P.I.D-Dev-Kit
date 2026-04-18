using System;
using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using Optional;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public interface ICommonEntityInterface : IEntity
    {
        public interface IHitBoxDefinition<T>
        {
            public IEnumerable<HitboxSlot<T>> HitBoxDefinition { get; }
        }
        public interface IVelocityProvider : ICommonEntityInterface
        {
            public IVelocityStat Velocity { get; }

            public interface IVelocityStat : IStatBase
            {
                public IVectorBase Value { get; }
            }
        }

        public interface IMovementProvider : ICommonEntityInterface
        {
            public IMovingStat MovingState { get; }

            public interface IMovingStat : IStatBase
            {
                public bool IsMoving { get; }
                public bool IsStanding => !IsMoving;
            }
        }

        public interface IMeleeAttackInputable : ICommonEntityInterface
        {
            public void InputMeleeAttack(IVectorBase vector);
            public void InputMeleeAttack();
        }

        public interface IMovementInputable : ICommonEntityInterface
        {
            public void InputMovement(IVectorBase vector);
        }

        public interface IJumpInputable : ICommonEntityInterface
        {
            public void InputMovement(float force);
        }

        public interface IPathFindable : ITargetContainable
        {
            public IVectorBase PathFindToTarget();
        }

        public interface ITargetContainable : ICommonEntityInterface
        {
            public Option<IEntity> Target { get; set; }
            public void ResetTarget() => Target = Option.None<IEntity>();
        }

        public interface ITargetDetectable : ITargetContainable
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

        public struct EntitySpawnFromAssetsEvent : ICommonEntityEvent
        {
            public EntitySpawnFromAssetsEvent(IEntity host)
            {
                Host = host;
            }

            public IEntity Host { get; }
        }

        public struct EntityDieEvent : ICommonEntityEvent
        {
            public EntityDieEvent(IEntity host, IDamage lethal)
            {
                Host = host;
                Lethal = lethal;
            }

            public IEntity Host { get; }
            public IDamage Lethal { get; }
        }

        public struct EntityTakeDamageEvent : ICommonEntityEvent
        {
            public EntityTakeDamageEvent(IEntity host, IDamage damage)
            {
                Host = host;
                Damage = damage;
            }

            public IEntity Host { get; }
            public IDamage Damage { get; }
        }
    }
}