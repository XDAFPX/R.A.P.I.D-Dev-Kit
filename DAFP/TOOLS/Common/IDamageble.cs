namespace DAFP.TOOLS.Common
{
    public interface IDamageble
    {
        public abstract void TakeDamage(Damage damage);
        public abstract void HealDamage(float heal);
    }
}