using UnityEngine;

namespace UGizmo
{
    // Contract for arrow drawing region
    public interface IGlobalGizmoArrow
    {
        void DrawArrow(Vector3 from, Vector3 to, Color color, float headLength = 0.5f, float width = 0.3f,
            float duration = 0f);

        void DrawArrow2d(Vector3 from, Vector3 to, Vector3 normal, Color color, float headLength = 0.5f,
            float width = 0.3f, float duration = 0f);

        void DrawFacingArrow2d(Vector3 from, Vector3 to, Color color, float headLength = 0.5f, float width = 0.3f,
            float duration = 0f);

        void DrawWireArrow(Vector3 from, Vector3 to, Vector3 normal, Color color, float headLength = 0.5f,
            float headWidth = 0.3f, float duration = 0f);

        void DrawFacingWireArrow(Vector3 from, Vector3 to, Color color, float headLength = 0.5f, float headWidth = 0.3f,
            float duration = 0f);
    }
}