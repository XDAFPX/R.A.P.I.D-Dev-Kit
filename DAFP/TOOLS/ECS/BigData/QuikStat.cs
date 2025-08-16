namespace DAFP.TOOLS.ECS.BigData
{
    public class QuikStat<T> : IStat<T>
    {
        public QuikStat(T value)
        {
            Value = value;
        }

        public string Name { get; set; }
        public void ResetToDefault()
        {
        }

        public bool SyncToBlackBoard => false; 
        public void Randomize(float margin01)
        {
        }

        public T Value { get; set; }
        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T DefaultValue { get; set; }
        public void SetToMax()
        {
        }

        public void SetToMin()
        {
        }

        public void AddModifier(StatModifier<T> modifier)
        {
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
        }
    }
}