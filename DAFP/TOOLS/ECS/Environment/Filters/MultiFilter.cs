using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    public class MultiFilterSO<T> : ScriptableObject, IFilter<T>
    {
        [Tooltip("Child filters evaluated with logical AND. All must pass.")]
        public SerializableInterface<IFilter<T>>[] children;

        public void Initialize(object self)
        {
            foreach (var _serializableInterface in children)
            {
                _serializableInterface.Value?.Initialize(self);
            }
        }

        public bool Evaluate(T go)
        {
            bool result = true;

            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i].Value;
                    if (child == null) continue; // null child is ignored
                    if (!child.Evaluate(go))
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
    }

    public class MultiFilter<T> : IFilter<T>
    {
        [Tooltip("Child filters evaluated with logical AND. All must pass.")]
        public SerializableInterface<IFilter<T>>[] children;

        public void Initialize(object self)
        {
            foreach (var _serializableInterface in children)
            {
                _serializableInterface.Value?.Initialize(self);
            }
        }

        public bool Evaluate(T go)
        {
            bool result = true;
            if (children == null) return result;
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (child == null) continue; // null child is ignored
                if (!child.Evaluate(go))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}