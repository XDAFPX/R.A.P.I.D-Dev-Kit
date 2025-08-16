using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData.Modifiers
{
    public class DamageMultiplierModifier : StatModifier<Damage>
    {
        private IStat<float> multiplier;
        public DamageMultiplierModifier(IEntity owner, IStat<float> multiplier) : base(owner)
        {
            this.multiplier = multiplier;
        }

        public override Damage Apply(Damage value)
        {
            return value * multiplier.Value;

        }

        public override int Priority { get; }
        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}