using DAFP.TOOLS.Common.Maths;

namespace DAFP.TOOLS.ECS.BigData.Modifiers
{
    public class ScaleVectorModifier : StatModifier<IVectorBase>
    {
        public IStat<float> Scaler { get; }
        private readonly float scaler;

        public ScaleVectorModifier(IEntity owner, IStat<float> scaler) : base(owner)
        {
            Scaler = scaler;
        }

        public ScaleVectorModifier(IEntity owner, float scaler) : base(owner)
        {
            this.scaler = scaler;
        }

        public override IVectorBase Apply(IVectorBase value)
        {
            if (Scaler != null)
                return value.Scale(Scaler.Value);
            return value.Scale(scaler);
        }

        public override int Priority => 10;

        public override void Dispose()
        {
        }
    }
}