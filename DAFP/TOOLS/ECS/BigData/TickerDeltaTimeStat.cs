namespace DAFP.TOOLS.ECS.BigData
{
    public class TickerDeltaTimeStat : IStat<ITickerBase>
    {
        public string Name { get; set; }
        public void ResetToDefault()
        {
            
        }

        private readonly ITickerBase based;

        public TickerDeltaTimeStat(ITickerBase based)
        {
            this.based = based;
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

        public ITickerBase Value
        {
            get => based;  set{} }
        public ITickerBase MaxValue { get; set; }
        public ITickerBase MinValue { get; set; }
        public ITickerBase DefaultValue { get; set; }
        public void SetToMax()
        {
        }

        public void SetToMin()
        {
        }

        public void AddModifier(StatModifier<ITickerBase> modifier)
        {
        }

        public void RemoveModifier(StatModifier<ITickerBase> modifier)
        {
        }
    }
}