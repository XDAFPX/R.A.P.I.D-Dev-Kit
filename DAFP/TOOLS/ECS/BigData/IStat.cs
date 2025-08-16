using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStat<T> : INameable, IStatBase, IRandomizeable
    
    {
        public T Value { get; set; }
        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T DefaultValue { get; set; }
        public void SetToMax();
        public void SetToMin();

        void AddModifier(StatModifier<T> modifier);
        void RemoveModifier(StatModifier<T> modifier);
    }
}