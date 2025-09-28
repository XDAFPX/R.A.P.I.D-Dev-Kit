using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEventBus;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.UIToolKit
{
    [RequireComponent(typeof(UIDocument))]
    public class ToolkitUISys<TElement> : MonoBehaviour, IUISystem<TElement> where TElement : IUIElement
    {
        public string Name
        {
            get => name;
            set => name = value;
        }

        [GetComponentCache] protected UIDocument Document;
        protected HashSet<TElement> Elements = new();


        public bool IsVisible
        {
            get { return Document.enabled; }
            set
            {
                if (value == IsVisible)
                    return;
                if (value)
                    Show();
                else
                    Hide();
            }
        }

        public event Action<bool, IUIElement> VisibilityChanged;

        private void Awake()
        {
            GetComponentCacheInitializer.InitializeCaches(this);
        }

        public void Show()
        {
            Document.enabled = true;
            VisibilityChanged?.Invoke(true, this);
        }

        public void Hide()
        {
            Document.enabled = false;
            VisibilityChanged?.Invoke(false, this);
        }

        public void Focus()
        {
        }

        public void UnFocus()
        {
        }

        public bool SetAttribute(string param, object value)
        {
            return false;
        }

        public void SubscribeToCallback(string name, Action<InputBind.CallbackContext> action)
        {
        }

        public void UnSubscribeToCallback(string name, Action<InputBind.CallbackContext> action)
        {
        }

        public void UnSubscribeFrom(string name)
        {
        }

        public void UnSubscribeAll()
        {
        }


        public void Register(TElement element)
        {
            if (element == null)
                return;

            element.VisibilityChanged += ElementOnVisibilityChanged;
            Elements.Add(element);
        }

        private void ElementOnVisibilityChanged(bool obj, IUIElement element)
        {
            ((IEventBus)Bus).Send(new VisibilityOnUIElementChanged(element, obj));
        }

        public UISystemEventBus Bus { get; } = new UISystemEventBus();

        HashSet<TElement> IUISystem<TElement>.GetElements()
        {
            return Elements;
        }
    }
}