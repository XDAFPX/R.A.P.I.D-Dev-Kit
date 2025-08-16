namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatDependent<T> : IStat<T>
    {
        public void Register(IDependentStat<T> owner);
    }
}