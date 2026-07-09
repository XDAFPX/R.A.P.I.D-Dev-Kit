using System;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnHurtBoxActivatedEvent<T> : IHurtBoxEvent
    {
        public OnHurtBoxActivatedEvent(HurtBox<T> hurtBox, HurtGroup<T> hurtGroup, HitBox<T> hitBox, T obj)
        {
            HurtBox = hurtBox;
            HurtGroup = hurtGroup;
            HitBox = hitBox;
            Obj = obj;
        }

        public HurtBox<T> HurtBox { get; }
        public HurtGroup<T> HurtGroup { get; }
        public HitBox<T> HitBox { get; }
        public T Obj { get; }

        IEntity IHurtBoxEvent.HurtBox => HurtBox;

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World),
                $"Hurtbox ({HurtBox}) of ({Obj}) was activated, by ({HitBox}) of group {HurtBox}");
        }
    }

    [LogLevel(LogLevel.Trace)]
    public struct OnHurtBoxActivatedEvent : IHurtBoxEvent
    {
        public IEntity HurtBox { get; }
        public INameable HurtGroup { get; }
        public IEntity HitBox { get; }
        public object Obj { get; }

        public OnHurtBoxActivatedEvent(IEntity hurtBox, INameable hurtGroup, IEntity hitBox, object obj)
        {
            HurtBox = hurtBox;
            HurtGroup = hurtGroup;
            HitBox = hitBox;
            Obj = obj;
        }


        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World),
                $"Generic hurtbox ({HurtBox}) of ({Obj}) was activated, by ({HitBox}) of group {HurtBox}");
        }

        public object ToGeneric(Type type)
        {
            var eventType = typeof(OnHurtBoxActivatedEvent<>).MakeGenericType(type);

            var hurtBox = CastTo(typeof(HurtBox<>).MakeGenericType(type), HurtBox);
            var hurtGroup = CastTo(typeof(HurtGroup<>).MakeGenericType(type), HurtGroup);
            var hitBox = CastTo(typeof(HitBox<>).MakeGenericType(type), HitBox);
            var obj = CastTo(type, Obj);

            return Activator.CreateInstance(eventType, hurtBox, hurtGroup, hitBox, obj);
        }

        private static object CastTo(Type type, object value)
        {
            // reflection-based casts don't need an actual cast operator,
            // just make sure the runtime value is assignable
            return value;
        }
    }
}