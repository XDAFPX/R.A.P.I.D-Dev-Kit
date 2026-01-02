using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Float
{
    [System.Serializable]
    public class DivideUintModifier : StatModifier<uint>
    {
        [SerializeField] private uint D;
        private IStat<uint> divider;

        public DivideUintModifier() : base(null)
        {
        }

        public DivideUintModifier(IEntity owner, IStat<uint> divider) : base(owner)
        {
            this.divider = divider;
        }

        public override uint Apply(uint value)
        {
            if (divider == null)
                return value / D;
            return value / divider.Value;
        }

        public override int Priority => 10;

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}