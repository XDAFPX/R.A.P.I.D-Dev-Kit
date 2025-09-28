using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.UI
{
    public interface IUIElement : INameable
    {
        public bool IsVisible { get; set; }
        public event Action<bool,IUIElement> VisibilityChanged;
        public void Show();
        public void Hide();
        public void Focus();
        public void UnFocus();
        public bool SetAttribute(string param, object value);
        public void SubscribeToCallback(string name, Action<InputBind.CallbackContext> action);
        public void UnSubscribeToCallback(string name, Action<InputBind.CallbackContext> action);
        public void UnSubscribeFrom(string name);
        public void UnSubscribeAll();
    }
}