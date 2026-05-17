using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.BuiltIn;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.Thinkers.IntegratedInput
{
    public class ControllerManager
    {
        private readonly List<IInputController> controllers = new();

        public T Create<T>(string name, InputActionAsset actions) where T : IInputController
        {
            var _controller = (T)Activator.CreateInstance(typeof(T), nonPublic: true);
            _controller.Init(name, actions);
            _controller.Enable();
            register(_controller);
            return _controller;
        }

        private void register(IInputController controller)
        {
            if (controller == null) return;
            controllers.Add(controller);
        }

        public IEnumerable<IInputController> Controllers => controllers;
    }
}