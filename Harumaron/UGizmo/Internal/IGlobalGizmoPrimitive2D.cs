using UnityEngine;

namespace UGizmo
{
    // Contract for 2D primitive drawing region
    public interface IGlobalGizmoPrimitive2D
    {
        // Circle2D
        void DrawCircle2D(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0f);
        void DrawCircle2D(Vector3 position, float radius, Color color, float duration = 0f);
        void DrawWireCircle2D(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0f);
        void DrawWireCircle2D(Vector3 position, float radius, Color color, float duration = 0f);

        // Box2D
        void DrawBox2D(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0f);
        void DrawBox2D(Vector3 position, float angle, Vector2 size, Color color, float duration = 0f);
        void DrawWireBox2D(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0f);
        void DrawWireBox2D(Vector3 position, float angle, Vector2 size, Color color, float duration = 0f);

        // Triangle2D
        void DrawTriangle2D(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0f);
        void DrawTriangle2D(Vector3 position, float angle, Vector2 size, Color color, float duration = 0f);
        void DrawWireTriangle2D(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0f);
        void DrawWireTriangle2D(Vector3 position, float angle, Vector2 size, Color color, float duration = 0f);
        void DrawWireTriangle2D(Vector3 point1, Vector3 point2, Vector3 point3, Color color, float duration = 0f);

        // Capsule2D
        void DrawCapsule2D(Vector3 center, Quaternion rotation, float height, float radius, Color color,
            float duration = 0f);

        void DrawCapsule2D(Vector3 center, float angle, float height, float radius, Color color, float duration = 0f);

        void DrawWireCapsule2D(Vector3 center, Quaternion rotation, float height, float radius, Color color,
            float duration = 0f);

        void DrawWireCapsule2D(Vector3 center, float angle, float height, float radius, Color color,
            float duration = 0f);
    }
}