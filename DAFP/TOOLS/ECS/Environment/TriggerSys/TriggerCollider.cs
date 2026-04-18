using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment
{
    /// <summary>
    /// Wrapper that can represent either a 3D Collider or a 2D Collider2D in a unified way.
    /// </summary>
    public readonly struct TriggerCollider
    {
        public readonly Collider Collider3D;
        public readonly Collider2D Collider2D;

        public TriggerCollider(Collider collider)
        {
            Collider2D = null;
            Collider3D = collider;
        }

        public TriggerCollider(Collider2D collider2D)
        {
            Collider3D = null;
            Collider2D = collider2D;
        }

        public bool Is2D => Collider2D != null;
        public bool Is3D => Collider3D != null;

        public Component Component => (Component)Collider3D ? (Component)Collider3D : Collider2D;
        public GameObject GameObject => Is3D ? Collider3D.gameObject : Collider2D != null ? Collider2D.gameObject : null;
        public Transform Transform => Is3D ? Collider3D.transform : Collider2D != null ? Collider2D.transform : null;

        public static implicit operator TriggerCollider(Collider c) => new TriggerCollider(c);
        public static implicit operator TriggerCollider(Collider2D c2d) => new TriggerCollider(c2d);
    }
}