using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace DAFP.TOOLS.ECS.UIToolKit
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class MonoUIToolKitElement<TToolKitElement> : MonoBehaviour, IUIElement
        where TToolKitElement : VisualElement
    {
        [field: SerializeField] public string Name { get; set; }

        [field: ReadOnly]
        [field: SerializeField]
        public bool IsVisible { get; set; }

        public event Action<bool, IUIElement> VisibilityChanged;
        protected Dictionary<string, (Func<object>, Action<object>)> Attributes;
        private UIDocument document;
        protected TToolKitElement Element;
        protected abstract Dictionary<string, (Func<object>, Action<object>)> GetInitialAttributes();

        private void Awake()
        {
            if (TryGetComponent(out IUISystem<IUIElement> sys)) sys.Register(this);
            document = GetComponent<UIDocument>();
            var a = document.rootVisualElement.Q<TToolKitElement>(Name);
            if (a != null)
            {
                Element = a;
            }
            else
            {
                throw new Exception($"An Element with Name: ({Name}) was not found (UI)");
                return;
            }

            Attributes = GetInitialAttributes();
            IsVisible = CurrentlyVisible();
        }

        protected virtual bool CurrentlyVisible()
        {
            return Element.visible;
        }

        public virtual void Show()
        {
            Element.visible = true;
        }

        public virtual void Hide()
        {
            Element.visible = false;
        }

        public virtual void Focus()
        {
            Element.Focus();
        }

        public virtual void UnFocus()
        {
            Element.Blur();
        }

        public bool SetAttribute(string param, object value)
        {
            if (!Attributes.ContainsKey(param))
                return false;
            if (Attributes.TryGetValue(param, out var entry))
                entry.Item2(value);
            return true;
        }

        public abstract void SubscribeToCallback(string name, Action<InputBind.CallbackContext> action);

        public abstract void UnSubscribeToCallback(string name, Action<InputBind.CallbackContext> action);

        public abstract void UnSubscribeFrom(string name);

        public abstract void UnSubscribeAll();
    }
}