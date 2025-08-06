using UnityEngine;

namespace DAFP.TOOLS.Common
{
    public static class Utills
    {
        public static Vector3 Randomize(this Vector3 vector3, float margin01)
        {
            vector3 = new Vector3(Random.Range(vector3.x * -margin01, vector3.x * margin01),
                Random.Range(vector3.y * -margin01, vector3.y * margin01),
                Random.Range(vector3.z * -margin01, vector3.z * margin01));
            return vector3;
        }

        public static float Randomize(this float value, float margin01)
        {
            value = Random.Range(value * -margin01, value * margin01);
            return value;
        }
    }
}