using System;
using System.Collections.Generic;
using UnityEngine;

namespace Derkii.FindByInterfaces
{
    public static class InterfaceFinder
    {
        public static FindRequestBuilder<T> FindByInterface<T>() where T : class
        {
#if DEBUG
            if (typeof(T).IsInterface == false) throw new Exception($"{typeof(T)} must be an interface");
#endif
            return new FindRequestBuilder<T>(UnityEngine.Object.FindObjectsOfType<Component>(),
                new List<Type> { typeof(T) });
        }
    }
}