using System;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters.LogicGates
{
    [Serializable]
    public class OrGateFilter<T> : IFilter<T>
    {
        [SerializeField] private SerializableInterface<IFilter<T>> Child;
        [SerializeField] private SerializableInterface<IFilter<T>> Child2;

        public bool Evaluate(T go)
        {
            if (Child2.Value == null)
                return false;
            if (Child.Value == null)
                return false;
            return Child.Value.Evaluate(go) || Child2.Value.Evaluate(go);
        }
    }
    [Serializable] public class OrGameObjectFilter : OrGateFilter<GameObject>{}
    [Serializable] public class OrEntityFilter : OrGateFilter<IEntity>{}
}