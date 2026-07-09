using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.Injection;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnModRegister : IObjectEvent
    {
        public OnModRegister(IMod mod)
        {
            Mod = mod;
        }

        public IMod Mod { get; }
        public object Object => Mod;

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(IModManager),
                $"Mod ({Mod}) was registered");
        }
    }
}