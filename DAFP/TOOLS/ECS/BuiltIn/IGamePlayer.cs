using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.Components;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public interface IGamePlayer : IEntity
    {
        public Dictionary<string, Action<InputAction.CallbackContext>> Binds { get; }
        public PlayerInput Input { get; }
        public AbstractUniversalInputController Controller { get; }

        public Dictionary<InputAction, Action<InputAction.CallbackContext>> BindedActions { get; }

        public void UnBindAll()
        {
            foreach (var _bindedAction in BindedActions)
            {
                _bindedAction.Key.performed -= _bindedAction.Value;
                _bindedAction.Key.canceled -= _bindedAction.Value;
                _bindedAction.Key.started -= _bindedAction.Value;
            }

            BindedActions.Clear();
            Binds.Clear();
        }

        public void Bind(string name, InputAction action, bool safe = false)
        {
            if (!Binds.ContainsKey(name))
            {
                if (!safe)
                    throw new Exception(
                        $"There is no bind with a name {name}. Please check if you are putting controller on a correct target");
                return;
            }

            // Handler wrapper that respects Controller.IsLocked
            void SafeInvoke(InputAction.CallbackContext ctx)
            {
                if (Controller.IsLocked)
                    return;
                Binds[name]?.Invoke(ctx);
            }

            BindedActions.Add(action, SafeInvoke);
            action.performed += SafeInvoke;
            action.started += SafeInvoke;
            action.canceled += SafeInvoke;
        }
    }
}