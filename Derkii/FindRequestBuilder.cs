using System;
using System.Collections.Generic;
using UnityEngine;

namespace Derkii.FindByInterfaces
{
    public class FindRequestBuilder<T> where T : class
    {
        private IEnumerable<Component> _components;
        private List<Type> _interfaceTypes;

        public FindRequestBuilder<TInterface> FindByInterface<TInterface>() where TInterface : class
        {
#if DEBUG
            if (typeof(TInterface).IsInterface == false) throw new ArgumentException($"{typeof(TInterface)} must be an interface");
#endif
            return new FindRequestBuilder<TInterface>(_components, _interfaceTypes);
        }

        public FindRequest Build() => new FindRequest(_components, _interfaceTypes);
        

        internal FindRequestBuilder(IEnumerable<Component> components, List<Type> interfaceTypes)
        {
            _components = components;
            _interfaceTypes = interfaceTypes;
            _interfaceTypes.Add(typeof(T));
        }
    }
}