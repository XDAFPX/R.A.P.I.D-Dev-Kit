namespace DAFP.TOOLS.ECS
{
    public interface IPlayer : IEntity
    {
        public bool IsLocal { get; set; }
        public void ToggleNoclip();
    }
}