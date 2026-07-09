using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using UnityEngine.Scripting;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [Preserve]
    public struct OnEntityRegisterEvent : IEntityEvent
    {
        public OnEntityRegisterEvent(IEntity entity)
        {
            Entity = entity;
        }

        public IEntity Entity { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(Adam), $"Entity ({Entity}) has been registered");
        }
    }
}