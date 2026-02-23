namespace DAFP.TOOLS.ECS.BigData.Modifiers
{
    public interface IPegStatModifier <T> : IStatModifier<T>
    {
        public IStatBase Peg { get; set; }
    }
}