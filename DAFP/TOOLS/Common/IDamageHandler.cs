using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IDamageHandler
    {
        public void TakeDamage(IDamage damage);
    }

    public interface IHealingHandler
    {
        public void TakeHealing(IHealing healing);
    }

    public interface IHealthHandler : IHealingHandler,IDamageHandler
    {
        
    }
}