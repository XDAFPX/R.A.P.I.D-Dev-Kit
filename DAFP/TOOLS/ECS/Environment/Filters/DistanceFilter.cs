using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [Serializable]
    public class DistanceFilter3D : IFilter<IEntity>, IFilter<GameObject>
    {
        [SerializeField] private float Distance;

        public DistanceFilter3D(float distance)
        {
            Distance = distance;
        }

        public DistanceFilter3D()
        {
        }

        private GameObject owner;

        void IFilter<GameObject>.Initialize(object self)
        {
            initialize_owner_from_self(self);
        }


        void IFilter<IEntity>.Initialize(object self)
        {
            initialize_owner_from_self(self);
        }

        private void initialize_owner_from_self(object self)
        {
            if (self is GameObject a)
                owner = a;
            if (self is IEntity ent)
                owner = ent.GetWorldRepresentation();
        }

        public bool Evaluate(IEntity go)
        {
            return Evaluate(go.GetWorldRepresentation());
        }

        public bool Evaluate(GameObject go)
        {
            if (owner == null)
                return false;
            return Vector3.Distance(go.transform.position, owner.transform.position) < Distance;
        }
    }

    [Serializable]
    public class DistanceFilter2D : IFilter<IEntity>, IFilter<GameObject>
    {
        [SerializeField] private float Distance;

        public DistanceFilter2D(float distance)
        {
            Distance = distance;
        }

        public DistanceFilter2D()
        {
        }

        private GameObject owner;

        void IFilter<GameObject>.Initialize(object self)
        {
            initialize_owner_from_self(self);
        }


        void IFilter<IEntity>.Initialize(object self)
        {
            initialize_owner_from_self(self);
        }

        private void initialize_owner_from_self(object self)
        {
            if (self is GameObject a)
                owner = a;
            if (self is IEntity ent)
                owner = ent.GetWorldRepresentation();
        }

        public bool Evaluate(IEntity go)
        {
            return Evaluate(go.GetWorldRepresentation());
        }

        public bool Evaluate(GameObject go)
        {
            if (owner == null)
                return false;
            return Vector2.Distance(go.transform.position, owner.transform.position) < Distance;
        }
    }
}