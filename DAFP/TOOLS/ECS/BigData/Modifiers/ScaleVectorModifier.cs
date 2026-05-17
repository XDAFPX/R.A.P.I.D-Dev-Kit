using DAFP.TOOLS.Common.Maths;

namespace DAFP.TOOLS.ECS.BigData.Modifiers
{
    public class ScaleVectorModifier : StatModifier<IVector>
    {
        public IStat<float> Scaler { get; }
        private readonly float scaler;

        public ScaleVectorModifier(IEntity owner, IStat<float> scaler, string name = nameof(ScaleVectorModifier)) :
            base(owner,name)
        {
            Scaler = scaler;
        }

        public ScaleVectorModifier(IEntity owner, float scaler, string name = nameof(ScaleVectorModifier)) : base(owner,name)
        {
            this.scaler = scaler;
        }

        public override IVector Apply(IVector value)
        {
            if (Scaler != null)
                return value.Scale(Scaler.Value);
            return value.Scale(scaler);
        }

        public override int Priority
        {
            get => 10;
            set { }
        }

        public override void Dispose()
        {
        }
    }
}