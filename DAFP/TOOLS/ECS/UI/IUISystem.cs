using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common.Utill;

namespace DAFP.TOOLS.ECS.UI
{
    public interface IUISystem<TElement> : IUIElement where TElement : IUIElement
    {
        public HashSet<TElement> GetElements();

        public TElement GetElement(string name)
        {
            return GetElements().FirstOrDefault(a => a.Name == name);
        }

        public void HideAll()
        {
            GetElements().ForEach((e) => e.Hide());
        }

        public void ShowAll()
        {
            GetElements().ForEach((e) => e.Show());
        }

        public void Register(TElement element);
        public UISystemEventBus Bus { get; }
    }
}