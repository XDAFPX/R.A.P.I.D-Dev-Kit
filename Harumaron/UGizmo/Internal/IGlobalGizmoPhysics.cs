using UnityEngine;

namespace UGizmo
{
    // Contract for physics-related gizmo helpers (3D/2D). Initially minimal; can be expanded.
    public interface IGlobalGizmoPhysics
    {
        // Physics (3D) examples
        bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);
        void DrawRaycast(Vector3 origin, Vector3 direction, float maxDistance, bool isHit, in RaycastHit hitInfo);

        bool Linecast(Vector3 start, Vector3 end, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);
        void DrawLinecast(Vector3 start, Vector3 end, bool isHit, in RaycastHit hitInfo);
    }
}
