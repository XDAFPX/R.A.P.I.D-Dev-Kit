using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.TriggerSys;
using DAFP.TOOLS.ECS.Services;
using UnityEngine.Scripting;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [Preserve]
    public struct OnTriggerActivatedEvent
    {
        public OnTriggerActivatedEvent(TriggerEntity triggerEntity, TriggerContext ctx)
        {
            TriggerEntity = triggerEntity;
            Ctx = ctx;
        }

        public TriggerEntity TriggerEntity { get; init; }
        public TriggerContext Ctx { get; init; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Trigger ({TriggerEntity}) was triggered, Context: {Ctx}");
        }
    }
}