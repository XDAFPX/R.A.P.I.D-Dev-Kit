using UnityEngine;

namespace DAFP.TOOLS.Common.Utill
{
    public static class RectTransformExtensions
    {
        // -----------------------------
        // Stretch helpers
        // -----------------------------
        public static void StretchFull(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void StretchHorizontal(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1, rect.anchorMax.y);
        }

        public static void StretchVertical(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, 0);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1);
        }

        // -----------------------------
        // Corner presets
        // -----------------------------
        public static void AnchorTopLeft(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
        }

        public static void AnchorTopCenter(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
        }

        public static void AnchorTopRight(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
        }

        public static void AnchorMiddleLeft(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
        }

        public static void AnchorMiddleCenter(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public static void AnchorMiddleRight(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
        }

        public static void AnchorBottomLeft(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
        }

        public static void AnchorBottomCenter(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
        }

        public static void AnchorBottomRight(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
        }

        // -----------------------------
        // Stretch with pivot helper
        // -----------------------------
        public static void StretchHorizontalWithPivot(this RectTransform rect, float pivotY = 0.5f)
        {
            rect.anchorMin = new Vector2(0, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1, rect.anchorMax.y);
            rect.pivot = new Vector2(0.5f, pivotY);
        }

        public static void StretchVerticalWithPivot(this RectTransform rect, float pivotX = 0.5f)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, 0);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1);
            rect.pivot = new Vector2(pivotX, 0.5f);
        }
    }
}