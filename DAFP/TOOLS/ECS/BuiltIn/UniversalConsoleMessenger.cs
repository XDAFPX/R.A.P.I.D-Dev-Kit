using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using UnityEditor;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalConsoleMessenger : IMessenger, ISwitchable
    {
        [Inject]public UniversalConsoleMessenger([Inject(Id = "ConsoleUnlocked")]bool unlocked)
        {
            ConsoleUnlocked = unlocked;
        }
        public bool ConsoleUnlocked;

        //TODO Implement All of that 
        public void Tick()
        {
        }

        public void Print(IMessage message)
        {
            throw new System.NotImplementedException();
        }

        public string Input()
        {
            throw new System.NotImplementedException();
        }

        public bool Enabled { set; get; }

        public void Enable()
        {
            Enabled = true;
            
        }

        public void Disable()
        {
            Enabled = false;
        }
    }
}