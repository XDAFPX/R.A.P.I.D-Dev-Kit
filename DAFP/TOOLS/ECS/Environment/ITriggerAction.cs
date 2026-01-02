namespace DAFP.TOOLS.ECS.Environment
{
    public interface ITriggerAction
    {
        void Act(TriggerEntity.TriggerEvent @event, TriggerCollider target);
    }
}