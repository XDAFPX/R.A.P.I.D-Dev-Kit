using DAFP.TOOLS.ECS.DebugSystem;

namespace DAFP.TOOLS.Common.TextSys
{
    public interface IMessenger : Zenject.ITickable, ICommandInterpreter
    {
        public void Print(IMessage message);
    }
}