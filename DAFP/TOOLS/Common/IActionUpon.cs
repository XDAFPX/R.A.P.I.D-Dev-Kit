namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IActionUpon<in T> : IAct
    {
        public void Act(T target);
        void IAct.Act()
        {
            Act(default);
        }
    }

    public interface IAct
    {
        public void Act();
    }
}