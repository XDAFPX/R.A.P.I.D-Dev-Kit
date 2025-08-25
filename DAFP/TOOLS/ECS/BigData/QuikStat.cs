namespace DAFP.TOOLS.ECS.BigData
{
    public class QuikStat<T> : IStat<T>
    {
        private T value;

        public QuikStat(T value)
        {
            Value = value;
        }

        public string Name { get; set; }

        public void ResetToDefault()
        {
        }

        public bool SyncToBlackBoard => false;

        public object GetAbsoluteValue()
        {
            return Value;
        }

        public event IStatBase.UpdateValueCallBack OnUpdateValue;

        public void Randomize(float margin01)
        {
        }

        public T Value
        {
            get => value;
            set { this.value = value; 
            OnUpdateValue?.Invoke(this);
        }
    }

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