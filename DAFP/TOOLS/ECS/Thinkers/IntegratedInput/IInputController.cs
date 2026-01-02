using System;
using DAFP.TOOLS.Common;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.Thinkers.IntegratedInput
{
    public interface IInputController : IDisposable, INameable, ISwitchable
    {
        void Bind(string move, Action<InputAction.CallbackContext> onMovePerformed);
    }
}