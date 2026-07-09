using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;
using UnityEngine.Scripting;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [LogLevel(LogLevel.Trace)] [Preserve]
    internal struct OnEntityDestroyedAsyncEvent : IEntityEvent
    {
        public OnEntityDestroyedAsyncEvent(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(World), $"Entity ({Entity}) async destroyed event fired");
        }
    }
}