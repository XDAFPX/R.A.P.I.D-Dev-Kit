using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Components;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public interface IBindedEntity : IEntity
    {
        public Dictionary<string, Action<InputBind.CallbackContext>> Binds { get; }
        public PlayerInput Input { get; }
        public AbstractUniversalInputController Controller { get; }

        public Dictionary<InputBind, Action<InputBind.CallbackContext>> BindedActions { get; }

        public void UnBindAll()
        {
            foreach (var _bindedAction in BindedActions)
            {
                
                _bindedAction.Key.performed -= _bindedAction.Value;
                _bindedAction.Key.canceled -= _bindedAction.Value;
                _bindedAction.Key.started -= _bindedAction.Value;
                _bindedAction.Key.Dispose();
            }

            BindedActions.Clear();
            Binds.Clear();
            
        }

        public void Bind(string name, InputBind action, bool safe = false)
        {
            if (!Binds.ContainsKey(name))
            {
                if (!safe)
                    throw new Exception(
                        $"There is no bind with a name {name}. Please check if you are putting controller on a correct target");
                return;
            }

            // Handler wrapper that respects Controller.IsLocked
            void SafeInvoke(InputBind.CallbackContext ctx)
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