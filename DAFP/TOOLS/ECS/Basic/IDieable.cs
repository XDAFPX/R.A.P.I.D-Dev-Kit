using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IDieable : IDeathHandler
    {
        bool Alive => !Dead;
        bool Dead { get; }
    }

    public interface IDeathHandler
    {
        void Die(IDamage lethal);
    }
}