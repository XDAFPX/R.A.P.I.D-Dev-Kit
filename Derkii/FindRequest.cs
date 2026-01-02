using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Derkii.FindByInterfaces
{
    public class FindRequest
    {
        private readonly IEnumerable<Component> _components;
        public IEnumerable<Component> Components => _components;

        public IEnumerable<T> ComponentsAsInterface<T>() where T : class
        {
#if UNITY_EDITOR
            if (typeof(T).IsInterface == false) throw new ArgumentException($"{typeof(T)} must be an interface");
#endif
            return _components.Where(t => t is T).Cast<T>();
        }

        internal FindRequest(IEnumerable<Component> components, IEnumerable<Type> interfaces)
        {
            _components = components.Where(component =>
            {
                byte same = 0;

                foreach (var @interface in interfaces)
                {
                    if (component.GetType().GetInterfaces().Contains(@interface)) same++;
                }

                return same == interfaces.Count();
            });
        }
    }
}