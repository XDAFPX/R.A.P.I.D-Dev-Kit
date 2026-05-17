using DAFP.TOOLS.ECS.Environment.DamageSys;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IDieable
    {
        bool Alive => !Dead;
        bool Dead { get; }

        IDieable Die(IDamage lethal);
    }
}
