using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment
{
    /// <summary>
    /// Wrapper that can represent either a 3D Collider or a 2D Collider2D in a unified way.
    /// </summary>
    public readonly struct UniversalCollider
    {
        public readonly Collider Collider3D;
        public readonly Collider2D Collider2D;

        public UniversalCollider(Collider collider)
        {
            Collider2D = null;
            Collider3D = collider;
        }

        public UniversalCollider(Collider2D collider2D)
        {
            Collider3D = null;
            Collider2D = collider2D;
        }

        public bool Is2D => Collider2D != null;
        public bool Is3D => Collider3D != null;

        public Component component => (Component)Collider3D ? (Component)Collider3D : Collider2D;

        public GameObject gameObject =>
            Is3D ? Collider3D.gameObject : Collider2D != null ? Collider2D.gameObject : null;

        public Transform transform => Is3D ? Collider3D.transform : Collider2D != null ? Collider2D.transform : null;

        public bool enabled
        {
            get
            {
                if (Collider2D != null)
                    return Collider2D.enabled;

                if (Collider3D != null)
                    return Collider3D.enabled;
                return false;
            }
            set
            {
                if (Collider2D != null)
                    Collider2D.enabled = value;

                if (Collider3D != null)
                    Collider3D.enabled = value;
            }
            
        }

        public static implicit operator UniversalCollider(Collider c) => new UniversalCollider(c);
        public static implicit operator UniversalCollider(Collider2D c2d) => new UniversalCollider(c2d);
    }
}