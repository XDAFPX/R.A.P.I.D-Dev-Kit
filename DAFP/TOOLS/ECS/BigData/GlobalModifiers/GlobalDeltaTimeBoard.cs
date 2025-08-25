using System;
using DAFP.TOOLS.ECS.BigData.Modifiers.Float;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public class GlobalDeltaTimeBoard : GlobalWhiteBoard<float,DeltaTimeModdable> 
    {
        protected override void OnTick()
        {
            
        }

        protected override float GetValue(float ProcessedValue)
        {
            return Host.EntityTicker.DeltaTime;
        }

        protected override void OnStart()
        {
            foreach (var _entityComponent in Host.Components.Values)
            {
                if (_entityComponent is IStatBase _stat && !ReferenceEquals(_entityComponent, this))
                    ApplyDamageModifiers(_stat);
            }
        }

        public override StatModifier<float>[] GetModifiers()
        {
            throw new NotImplementedException();
        }

        private void ApplyDamageModifiers(IStatBase dmg)
        {
            if (dmg.GetType().GetCustomAttributes(typeof(DeltaTimeModdable), true).Length > 0)
            {
                if (dmg is IStat<float> stat)
                {
                    
                    // Confusing... But this just adds a multiplier for deltatime for a bord. And it gets a ticker to get the deltatime specified in the component its self bia EntityComponentTicker
                    stat.AddModifier(new MultiplyFloatModifier(new TickerDeltaTimeStat(((IEntityComponent)(dmg)).EntityComponentTicker),Host));
                    
                }

            }
        }

        public override bool SyncToBlackBoard => false; 
        protected override void OnInitializeInternal()
        {
        }

        protected override float ClampAndProcessValue(float value)
        {
            return value;
        }

        public override void Randomize(float margin01)
        {
        }
    }

    public class DeltaTimeModdable : Attribute
    {
    }
}