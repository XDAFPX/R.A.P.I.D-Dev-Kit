using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData.Health;
using DAFP.TOOLS.ECS.BigData.Modifiers.Float;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public abstract class GlobalFloatMultiplyBoard<TAttribute> : GlobalWhiteBoard<float, TAttribute> where TAttribute : System.Attribute
    {
        public override float MaxValue => 1f;
        public override float MinValue => 0f;

        [ReadOnly(OnlyWhilePlaying = true)][Range(0, 1)]
        [SerializeField]
        private float _defaultValue = 1f;

        public override float DefaultValue
        {
            get => _defaultValue;
            set => _defaultValue = value;
        }

        protected override void OnTick()
        {
            // Optionally override in derived classes
        }

        protected override void OnInitializeInternal()
        {
            // Optionally override in derived classes
        }

        protected override float ClampAndProcessValue(float value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }

        public override void Randomize(float margin01)
        {
        }

        public override bool SyncToBlackBoard => true;

        public override StatModifier<float>[] GetModifiers()
        {
            return new[] { new MultiplyFloatModifier(this,Host) };
        }
    }
}