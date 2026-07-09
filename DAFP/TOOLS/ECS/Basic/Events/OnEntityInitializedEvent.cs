using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityInitializedEvent
    {
        public readonly IEntity Entity;

        public OnEntityInitializedEvent(IEntity entity)
        {
            Entity = entity;
        }

        public override string ToString()
        {
            return GameUtils.FormatLog(Entity.GetWorld(), $"Entity ({Entity}) has been initialized");
        }
    }
}