using UnityEngine;

namespace UGizmo
{
    // Contract for physics-related 2D gizmo helpers
    public interface IGlobalGizmoPhysics2D
    {
        // Raycast2D
        RaycastHit2D Raycast2D(Vector2 origin, Vector2 direction, float distance = float.PositiveInfinity,
            int layerMask = -1, float minDepth = float.MinValue, float maxDepth = float.MaxValue);

        RaycastHit2D Raycast2D(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter2D,
            float distance = float.PositiveInfinity);

        // Linecast2D
        RaycastHit2D Linecast2D(Vector2 start, Vector2 end, float distance = float.PositiveInfinity, int layerMask = -1,
            float minDepth = float.MinValue, float maxDepth = float.MaxValue);

        RaycastHit2D Linecast2D(Vector2 start, Vector2 end, ContactFilter2D contactFilter2D,
            float distance = float.PositiveInfinity);

        // CircleCast2D
        RaycastHit2D CircleCast2D(Vector2 origin, float radius, Vector2 direction,
            float distance = float.PositiveInfinity, int layerMask = -1, float minDepth = float.MinValue,
            float maxDepth = float.MaxValue);

        RaycastHit2D CircleCast2D(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter2D,
            float distance = float.PositiveInfinity);

        // BoxCast2D
        RaycastHit2D BoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction,
            float distance = float.PositiveInfinity, int layerMask = -1, float minDepth = float.MinValue,
            float maxDepth = float.MaxValue);

        RaycastHit2D BoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction,
            ContactFilter2D contactFilter2D, float distance = float.PositiveInfinity);

        // CapsuleCast2D
        RaycastHit2D CapsuleCast2D(Vector2 origin, Vector2 size, float angle, CapsuleDirection2D capsuleDirection,
            Vector2 direction, float distance = float.PositiveInfinity, int layerMask = -1,
            float minDepth = float.MinValue, float maxDepth = float.MaxValue);

        RaycastHit2D CapsuleCast2D(Vector2 origin, Vector2 size, float angle, CapsuleDirection2D capsuleDirection,
            Vector2 direction, ContactFilter2D contactFilter2D, float distance = float.PositiveInfinity);
    }
}