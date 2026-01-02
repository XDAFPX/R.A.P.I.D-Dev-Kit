namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IContainerOf<out T>
    {
        public T GetContents();
    }
}