using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys
{
    public interface ITriggerAction : IActionUpon<TriggerContext>
    {
    }

    public struct TriggerContext
    {
        public TriggerContext( TriggerEntity.TriggerEvent @event, UniversalCollider target)
        {
            Target = target;
            Event = @event;
        }

        public TriggerEntity.TriggerEvent Event { get; }
        public UniversalCollider Target { get; }
    }
}