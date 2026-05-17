using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public interface IHealing : IHealthChange<HealingInfo>
    {
    }

    public class Healing : IHealing
    {
        public Healing(HealingInfo info)
        {
            Info = info;
        }

        public HealingInfo Info { get; set; }
    }
}
