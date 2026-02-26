using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Environment
{
    public interface ITriggerAction : IActionUpon<TriggerContext>
    {
    }

    public struct TriggerContext
    {
        public TriggerContext( TriggerEntity.TriggerEvent @event, TriggerCollider target)
        {
            Target = target;
            Event = @event;
        }

        public TriggerEntity.TriggerEvent Event { get; }
        public TriggerCollider Target { get; }
    }
}