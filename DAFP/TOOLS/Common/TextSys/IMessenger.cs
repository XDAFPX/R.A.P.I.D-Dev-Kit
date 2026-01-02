using DAFP.TOOLS.ECS.DebugSystem;

namespace DAFP.TOOLS.Common.TextSys
{
    public interface IMessenger : Zenject.ITickable, ICommandInterpriter
    {
        public void Print(IMessage message);
    }
}