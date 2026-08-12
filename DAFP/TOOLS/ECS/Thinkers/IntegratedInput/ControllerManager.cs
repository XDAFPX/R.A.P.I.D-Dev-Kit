using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.BuiltIn;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.Thinkers.IntegratedInput
{
    public class ControllerManager : IDisposable
    {
        private readonly List<IInputController> controllers = new();

        public virtual T Create<T>(string name, InputActionAsset actions) where T : IInputController
        {
            var _controller = (T)Activator.CreateInstance(typeof(T), nonPublic: true);
            _controller.Init(name, actions);
            _controller.Enable();
            Register(_controller);
            
            return _controller;
        }

        protected virtual void Register(IInputController controller)
        {
            if (controller == null) return;
            controllers.Add(controller);
        }

        public virtual void Dispose()
        {
            foreach (var _inputController in controllers)
            {
                _inputController.Dispose();
            }
        }

        public IEnumerable<IInputController> Controllers => controllers;
    }
}