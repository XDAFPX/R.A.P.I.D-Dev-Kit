namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IActionUpon<in T>
    {
        public void Act(T target);
    }
}