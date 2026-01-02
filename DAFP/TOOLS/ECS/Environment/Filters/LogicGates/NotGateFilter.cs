using System;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters.LogicGates
{
    [Serializable]
    public class NotGateFilter<T>: IFilter<T>
    {
        [SerializeField] private SerializableInterface<IFilter<T>> Child;
        public bool Evaluate(T go)
        {
            if (Child.Value == null)
                return false;
            return !Child.Value.Evaluate(go);
        }
    } 
    [Serializable] public class NotGameObjectFilter : NotGateFilter<GameObject>{}
    [Serializable] public class NotEntityFilter : NotGateFilter<IEntity>{}
}