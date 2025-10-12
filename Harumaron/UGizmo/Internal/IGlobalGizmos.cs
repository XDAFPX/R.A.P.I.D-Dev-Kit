using UnityEngine;

namespace UGizmo
{
    // Primary contract for global gizmo drawing. Initially minimal; can be expanded to cover all regions.
    public interface IGlobalGizmos : IGlobalGizmoPrimitive,IGlobalGizmoPrimitive2D,IGlobalGizmoUtility
    {
        // Primitive examples
        void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f);
        void DrawWireSphere(Vector3 center, float radius, Color color, float duration = 0f);
        void DrawWireSphere(Vector3 center, Quaternion rotation, float radius, Color color, float duration = 0f);

        void DrawCube(Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f);
        void DrawWireCube(Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f);

        // Arrows (commonly used)
        void DrawArrow(Vector3 from, Vector3 to, Color color, float headLength = 0.5f, float width = 0.15f,
            float duration = 0f);

        void DrawArrow2d(
            Vector3 from,
            Vector3 to,
            Vector3 normal,
            Color color,
            float headLength = 0.5f,
            float width = 0.15f,
            float duration = 0f);

        void DrawWireArrow(
            Vector3 from,
            Vector3 to,
            Vector3 normal,
            Color color,
            float headLength = 0.5f,
            float headWidth = 0.3f,
            float duration = 0f);

        // Lines
        void DrawLine(Vector3 from, Vector3 to, Color color, float duration = 0f);
    }
}