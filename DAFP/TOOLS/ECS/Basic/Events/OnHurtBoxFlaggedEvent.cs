using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [LogLevel(LogLevel.Trace)]
    public struct OnHurtBoxFlaggedEvent : IHurtBoxEvent
    {
        public OnHurtBoxFlaggedEvent(IEntity hurtBox)
        {
            HurtBox = hurtBox;
        }

        public IEntity HurtBox { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World),
                $"Hurtbox ({HurtBox}) was flagged");
        }
    }
}