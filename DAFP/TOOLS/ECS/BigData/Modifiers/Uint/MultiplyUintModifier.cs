using Archon.SwissArmyLib.Utils.Editor;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Float
{
    [System.Serializable]
    public class MultiplyUintModifier : StatModifier<uint>
    {
        [SerializeField] private uint M;
        private IStat<uint> multiplier;


        public MultiplyUintModifier() : base(null)
        {
        }


        public MultiplyUintModifier(IStat<uint> multiplier, IEntity owner) : base(owner)
        {
            this.multiplier = multiplier;
        }

        public override uint Apply(uint value)
        {
            if (multiplier == null)
                return value * M;
            return value * (multiplier?.Value ?? 1);
        }

        public override int Priority => 10;

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}