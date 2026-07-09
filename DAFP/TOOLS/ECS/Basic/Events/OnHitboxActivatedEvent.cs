using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnHitBoxActivatedEvent<T>
    {
        public HitBox<T> Hitbox { get; init; }
        public IEntity Owner { get; init; }
        public IFilter<T>[] FiltersActivated { get; init; }
        public IActionUpon<T>[] ActionsDone { get; init; }
        public T[] StuffCaught { get; init; }
        public HurtBox<T>[] HurtBoxesFound { get; init; }


        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World),
                $"HitBox of ({Owner}) was activated, staff caught: {StuffCaught.Length}, hurtboxes found {HurtBoxesFound.Length}");
        }
    }

    [LogLevel(LogLevel.Trace)]
    public struct OnHitBoxActivatedEvent
    {
        public IEntity Hitbox;
        public IEntity Owner;
        public object[] StuffCaught;

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World),
                $"Generic hitBox of ({Owner}) was activated, staff caught: {StuffCaught.Length}");
        }
    }
}