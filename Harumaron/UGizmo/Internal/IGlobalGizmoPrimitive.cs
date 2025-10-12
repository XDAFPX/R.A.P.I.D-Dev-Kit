using Unity.Mathematics;
using UnityEngine;

namespace UGizmo
{
    // Contract for 3D primitive drawing region
    public interface IGlobalGizmoPrimitive
    {
        // Sphere
        void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f);
        void DrawWireSphere(Vector3 center, float radius, Color color, float duration = 0f);
        void DrawWireSphere(Vector3 center, Quaternion rotation, float radius, Color color, float duration = 0f);

        // Cube
        void DrawCube(Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f);
        void DrawWireCube(Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f);

        // Capsule
        void DrawCapsule(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0f);
        void DrawCapsule(Vector3 center, Vector3 upAxis, float height, float radius, Color color, float duration = 0f);
        void DrawCapsule(Vector3 point1, Vector3 point2, float radius, Color color, float duration = 0f);

        void DrawWireCapsule(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0f);

        void DrawWireCapsule(Vector3 center, Vector3 upAxis, float height, float radius, Color color,
            float duration = 0f);

        void DrawWireCapsule(Vector3 point1, Vector3 point2, float radius, Color color, float duration = 0f);

        // Cylinder
        void DrawCylinder(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0f);
        void DrawCylinder(Vector3 point1, Vector3 point2, float radius, Color color, float duration = 0f);
        void DrawWireCylinder(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0f);
        void DrawWireCylinder(Vector3 point1, Vector3 point2, float radius, Color color, float duration = 0f);

        // Cone
        void DrawCone(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0f);
        void DrawCone(float3 origin, float3 direction, float distance, float angle, Color color, float duration = 0f);
        void DrawWireCone(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0f);

        void DrawWireCone(float3 origin, float3 direction, float distance, float angle, Color color,
            float duration = 0f);

        // Plane
        void DrawPlane(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0f);
        void DrawWirePlane(Vector3 position, Quaternion rotation, Vector2 size, Color color, float duration = 0f);
    }
}