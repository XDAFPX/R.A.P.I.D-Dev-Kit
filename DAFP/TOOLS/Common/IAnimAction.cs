using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace RapidLib.DAFP.TOOLS.Common
{
        public interface IAnimAction
        {
            public readonly struct HurtAction : IAnimAction
            {
                public readonly DamageInfo Info;
                public HurtAction(in DamageInfo info) => Info = info;
            }

            public readonly struct HealAction : IAnimAction
            {
                public readonly HealingInfo Info;
                public HealAction(in HealingInfo info) => Info = info;
            }
        }
}