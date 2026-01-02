using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.Thinkers.IntegratedInput
{
    public class InputController : IInputController
    {
        private readonly InputActionAsset actions;
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> callbacks = new();

        public InputController(string name, InputActionAsset actions)
        {
            Name = name;
            this.actions = actions;
        }

        public void Bind(string actionName, Action<InputAction.CallbackContext> callback)
        {
            var _action = actions.FindAction(actionName, true);
            callbacks[actionName] = callback;

            if (Enabled)
                subscribe(_action, callback);
        }


        private static void subscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.started += callback;
            action.performed += callback;
            action.canceled += callback;
        }

        private static void unsubscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.started -= callback;
            action.performed -= callback;
            action.canceled -= callback;
        }

        public void Dispose()
        {
            Disable();
            callbacks.Clear();
        }

        public string Name { get; set; }
        public bool Enabled { get; private set; }

        public void Enable()
        {
            handle_switch_logic(true);
        }

        public void Disable()
        {
            handle_switch_logic(false);
        }

        private void handle_switch_logic(bool enabled)
        {
            if (Enabled == enabled) return;
            Enabled = enabled;

            foreach (var _pair in callbacks)
            {
                var _action = actions.FindAction(_pair.Key, false);
                if (_action == null) continue;

                if (enabled) subscribe(_action, _pair.Value);
                else unsubscribe(_action, _pair.Value);
            }
        }
    }
}