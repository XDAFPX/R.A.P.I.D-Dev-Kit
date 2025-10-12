using UnityEngine;

namespace DAFP.TOOLS.Common
{
    [System.Serializable]
    public class AngleClamper
    {
        [SerializeField]private  float Min;
        [SerializeField]private float Max;

        public AngleClamper(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public static AngleClamper FREE = new AngleClamper(-1000, -1000);

        public float Clamp(float angle)
        {
            if (Min == -1000 && Max == -1000)
                return angle;
            return ClampAngle(angle, Min, Max);
        }
        public Vector2 Clamp(Vector2 dir)
        {
            if (dir == Vector2.zero)
                return dir;
            if (Min == -1000 && Max == -1000)
                return dir;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = NormalizeAngle360(angle);
            var fromAngleDeg = NormalizeAngle360(Max);
            var toAngleDeg = NormalizeAngle360(Min);

            bool IsBetween(float a, float min, float max)
            {
                if (min <= max)
                    return a >= min && a <= max;
                return a >= min || a <= max; // wraparound
            }

            if (IsBetween(angle, fromAngleDeg, toAngleDeg))
                return dir.normalized;

            float deltaFrom = Mathf.DeltaAngle(angle, fromAngleDeg);
            float deltaTo = Mathf.DeltaAngle(angle, toAngleDeg);
            float closest = Mathf.Abs(deltaFrom) < Mathf.Abs(deltaTo) ? fromAngleDeg : toAngleDeg;

            float rad = closest * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        }
        public static float NormalizeAngle360(float angle)
        {
            angle %= 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }
        private static float GetNearestAngleInsideRange(float angle, float min, float max)
        {
            // Wrap all angles
            angle = NormalizeAngle(angle);
            min = NormalizeAngle(min);
            max = NormalizeAngle(max);

            float deltaToMin = Mathf.DeltaAngle(angle, min);
            float deltaToMax = Mathf.DeltaAngle(angle, max);

            // We take the one with smaller absolute distance
            return Mathf.Abs(deltaToMin) < Mathf.Abs(deltaToMax) ? min : max;
        }
        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }

        public static bool IsAngleBetween(float angle, float min, float max)
        {
            angle = NormalizeAngle(angle);
            min = NormalizeAngle(min);
            max = NormalizeAngle(max);

            if (min < max)
                return angle >= min && angle <= max;
            else
                return angle >= min || angle <= max;
        }
        public static float ClampAngle(float angle, float from, float to)
        {
            // accepts e.g. -80, 80
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);

        }

        public override string ToString() => $"Clamp[{Min}°, {Max}°]";
    }

}