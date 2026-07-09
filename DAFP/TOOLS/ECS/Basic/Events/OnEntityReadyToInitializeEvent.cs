using DAFP.TOOLS.Common.Utill;
using Microsoft.Extensions.Logging;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [LogLevel(LogLevel.Trace)]
    internal struct OnEntityReadyToInitializeEvent : IEntityEvent
    {
        public OnEntityReadyToInitializeEvent(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(Entity.GetWorld(), $"Entity ({Entity}) is ready to be intialized");
        }
    }
}