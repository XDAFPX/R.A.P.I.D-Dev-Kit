using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Thinkers;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityThinkerChangedEvent : IEntityEvent
    {
        public OnEntityThinkerChangedEvent(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; }
        public IThinker NewThinkker => Entity.Brains;
        public override string ToString()
        {
            return GameUtils.FormatLog(Entity.GetWorld(), $"Entity ({Entity}) has changed thinker to {Entity.Brains}");
        }
    }
}