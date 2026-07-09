using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;
using UnityEngine.Scripting;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [LogLevel(LogLevel.Trace)] [Preserve]
    internal struct OnEntityCreatedAsyncEvent : IEntityEvent
    {
        public OnEntityCreatedAsyncEvent(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(Adam), $"Entity ({Entity}) async created event fired");
        }
    }
}