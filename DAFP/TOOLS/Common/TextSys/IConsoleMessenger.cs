using DAFP.TOOLS.ECS.DebugSystem;

namespace DAFP.TOOLS.Common.TextSys
{
    public interface IConsoleMessenger : IMessenger, Zenject.ITickable, ICommandInterpreter
    {
        public void Clear();
    }

    public interface IMessenger 
    {
        public void Print(IMessage message);
    }
}