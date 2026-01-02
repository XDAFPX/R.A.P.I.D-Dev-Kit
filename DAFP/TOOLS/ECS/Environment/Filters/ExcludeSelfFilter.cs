using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [Serializable]
    public class ExcludeSelfFilter : IFilter<IEntity>, IFilter<GameObject>
    {
        private GameObject myself;
        private IEntity myself2;

        void IFilter<GameObject>.Initialize(object self)
        {
            initialize_self_references(self);
        }


        void IFilter<IEntity>.Initialize(object self)
        {
            initialize_self_references(self);
        }

        private void initialize_self_references(object self)
        {
            switch (self)
            {
                case GameObject s:
                    myself = s;
                    break;
                case IEntity s2:
                    myself2 = s2;
                    break;
            }
        }

        public bool Evaluate(IEntity go)
        {
            if (myself2 != null)
                return go.GetWorldRepresentation() != myself2.GetWorldRepresentation();
            if (myself != null)
                return go.GetWorldRepresentation() != myself;
            return false;
        }

        public bool Evaluate(GameObject go)
        {
            if (myself != null)
                return go != myself;
            return false;
        }
    }
}