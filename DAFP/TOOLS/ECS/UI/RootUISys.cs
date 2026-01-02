using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace DAFP.TOOLS.ECS.UI
{
    public class RootUISys : IUISystem<IUISystem<IUIElement>>

    {
        public string Name { get; set; } = "Root";

        public bool IsVisible
        {
            get => true;
            set { }
        }

        public event Action<bool, IUIElement> VisibilityChanged;
        private HashSet<IUISystem<IUIElement>> Systems;

        public RootUISys(IEnumerable<IUISystem<IUIElement>> systems)
        {
            Systems = systems.ToHashSet();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }

        public void Focus()
        {
            throw new NotImplementedException();
        }

        public void UnFocus()
        {
            throw new NotImplementedException();
        }

        public bool SetAttribute(string param, object value)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToCallback(string name, Action<InputBind.CallbackContext> action)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeToCallback(string name, Action<InputBind.CallbackContext> action)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeFrom(string name)
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeAll()
        {
            throw new NotImplementedException();
        }


        public HashSet<IUISystem<IUIElement>> GetElements()
        {
            return Systems;
        }

        public IUISystem<IUIElement> GetElement(string name)
        {
            return GetElements().FirstOrDefault(a => a.Name.Contains(name));
        }

        public void Register(IUISystem<IUIElement> element)
        {
            Systems.Add(element);
        }

        public UISystemEventBus Bus { get; }
    }
}