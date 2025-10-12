namespace DAFP.TOOLS.Common.TextSys
{
    public interface IMessenger : Zenject.ITickable
    {
        public void Print(IMessage message);
        public string Input();
    }
}