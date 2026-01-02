using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [Serializable]
    public class EmptyGOFilter : EmptyFilter<GameObject>{}

    public class EmptyEntFilter : EmptyFilter<IEntity>{}

    public class EmptyFilter<T> : IFilter<T>
    {
        public bool Evaluate(T go)
        {
            return true;
        }
    }
}