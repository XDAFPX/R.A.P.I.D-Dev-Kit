using System;
using DAFP.TOOLS.ECS.BuiltIn;

namespace DAFP.TOOLS.ECS.Environment.DamageSys
{
    public interface IDamageable : IEntity
    {
        public IDamageable TakeDamage(IDamage damage);
        public event Action<ICommonEntityEvent.EntityTakeDamageEvent> OnTakeDamage;
    }
}