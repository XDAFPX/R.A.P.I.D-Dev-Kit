using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public interface IInputController
    {
        public static List<IInputController> Controllers = new();
        public bool IsLocked { get; }
        public void Lock();
        public void UnLock();
        public PlayerInput Input { get; }
        public void OnRegisterController(IGamePlayer player);
    }
}