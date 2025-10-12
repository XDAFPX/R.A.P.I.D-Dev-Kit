using System;
using UnityEngine;

namespace UGizmo
{
    // Contract for utility drawing region
    public interface IGlobalGizmoUtility
    {
        // Points and lines
        void DrawPoint(Vector3 position, float radius, Color color, float duration = 0f);
        void DrawLine(Vector3 from, Vector3 to, Color color, float duration = 0f);
        unsafe void DrawLineList(ReadOnlySpan<Vector3> points, Color color, float duration = 0f);
        unsafe void DrawLineStrip(ReadOnlySpan<Vector3> points, bool loop, Color color, float duration = 0f);

        // Rays
        void DrawRay(Vector3 from, Vector3 direction, Color color, float duration = 0f);
        void DrawRay(Ray ray, Color color, float duration = 0f);

        // Frustum and distance/measure
        void DrawFrustum(Vector3 center, Quaternion rotation, float fov, float farClipPlane, float nearClipPlane,
            float aspect, Color color, float duration = 0f);

        void DrawFrustum(Camera camera, Color color, float duration = 0f);

        void DrawDistance(Vector3 from, Vector3 to, Color color, float headLength = 0.5f, float headWidth = 0.3f,
            float duration = 0f);
    }
}