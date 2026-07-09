namespace DAFP.TOOLS.ECS.Basic
{
    public interface IHurtBoxEvent : IEntityEvent
    {
        IEntity HurtBox { get; }
        IEntity IEntityEvent.Entity => HurtBox;
    }
}