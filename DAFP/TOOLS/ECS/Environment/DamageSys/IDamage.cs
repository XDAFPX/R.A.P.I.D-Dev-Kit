using System;
using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public interface IDamage : IHealthChange<DamageInfo>
    {
    }

    public class Damage : IDamage
    {

        public Damage(DamageInfo info)
        {
            Info = info;
        }

        public DamageInfo Info { get; set; }
    }
}