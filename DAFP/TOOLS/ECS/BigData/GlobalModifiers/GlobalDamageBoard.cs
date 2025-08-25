using System;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public class GlobalDamageBoard : GlobalFloatMultiplyBoard<DamageModdable>
    {
        protected override void OnStart()
        {
            foreach (var _entityComponent in Host.Components.Values)
            {
                if (_entityComponent is IStat<Damage> _stat && !ReferenceEquals(_entityComponent, this))
                    ApplyDamageModifiers(_stat);
            }
        }

        private void ApplyDamageModifiers(IStat<Damage> dmg)
        {
            if (dmg.GetType().GetCustomAttributes(typeof(DamageModdable), true).Length > 0)
                dmg.AddModifier(new DamageMultiplierModifier(Host, this));
        }
    }

    public class DamageModdable : Attribute
    {
    }
}