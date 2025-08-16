namespace DAFP.TOOLS.ECS.BigData.Modifiers.Float
{
    public class DivideFloatModifier : StatModifier<float>
    {
        private IStat<float> divider;
        public DivideFloatModifier(IEntity owner, IStat<float> divider) : base(owner)
        {
            this.divider = divider;
        }

        public override float Apply(float value)
        {
            return value / divider.Value;
            
        }

        public override int Priority => 10;
        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}