using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DAFP.TOOLS.ECS.Serialization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.Common.Utill
{
    public static class Utills
    {

        public static Vector3 Randomize(this Vector3 vector3, float margin01)
        {
            vector3 += new Vector3(Random.Range(vector3.x * -margin01, vector3.x * margin01),
                Random.Range(vector3.y * -margin01, vector3.y * margin01),
                Random.Range(vector3.z * -margin01, vector3.z * margin01));
            return vector3;
        }

        public static float Randomize(this float value, float margin01)
        {
            value += Random.Range(value * -margin01, value * margin01);
            return value;
        }

        public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value)
        {
            foreach (var kvp in dict)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                    return kvp.Key;
            }

            throw new Exception("Value not found in dictionary.");
        }

        public static Vector3 Add(this Vector3 v3, Vector2 v2)
        {
            return v3 + new Vector3(v2.x, v2.y, 0);
        }

        public static Vector3 Add(this Vector3 v3, Vector3 v32)
        {
            return v3 + v32;
        }

        public static Vector3 ClampMinMagnitude(this Vector3 vector, float minLength)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            float minLengthSqr = minLength * minLength;

            // If vector is already longer than minLength or it's zero, return as is
            if (sqrMagnitude >= minLengthSqr || sqrMagnitude == 0f)
                return vector;

            float magnitude = Mathf.Sqrt(sqrMagnitude);
            float scale = minLength / magnitude;

            return new(vector.x * scale, vector.y * scale, vector.z * scale);
        }

        public static Vector2 ClampMinMagnitude(this Vector2 vector, float minLength)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            float minLengthSqr = minLength * minLength;

            // If vector is already longer than minLength or it's zero, return as is
            if (sqrMagnitude >= minLengthSqr || sqrMagnitude == 0f)
                return vector;

            float magnitude = Mathf.Sqrt(sqrMagnitude);
            float scale = minLength / magnitude;

            return new Vector2(vector.x * scale, vector.y * scale);
        }


        public static List<AnimationClip> GetAllAnimationClips(this Animator animator) =>
            _GetAllAnimationClips(animator);

        public static List<AnimationClip> _GetAllAnimationClips(Animator animator)
        {
            var clips = new List<AnimationClip>();
            if (animator == null || animator.runtimeAnimatorController == null)
                return clips;

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;

            // Check if it's an override controller
            if (controller is AnimatorOverrideController overrideController)
            {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);

                foreach (var pair in overrides)
                {
                    if (pair.Value != null && !clips.Contains(pair.Value))
                        clips.Add(pair.Value);
                }
            }
            else
            {
                // Default case
                clips.AddRange(controller.animationClips);
            }

            return clips;
        }

        public static bool IsPointInCone(Vector2 origin, Vector2 direction, float angle, float length, Vector2 point)
        {
            Vector2 toPoint = point - origin;

            // Outside range
            if (toPoint.magnitude > length)
                return false;

            // Check angle
            float angleToTarget = Vector2.Angle(direction.normalized, toPoint.normalized);
            return angleToTarget <= angle;
        }

        public static T[] GetComponentsInRoot<T>(this GameObject root) where T : Component
        {
            return _GetComponentsInRoot<T>(root.transform);
        }

        public static T[] GetComponentsInRoot<T>(this Transform root) where T : Component
        {
            return _GetComponentsInRoot<T>(root);
        }

        public static T[] _GetComponentsInRoot<T>(Transform root) where T : Component
        {
            List<T> _components = new List<T>();

            // Add components on the root object itself
            root.GetComponents(_components);

            // Traverse all children recursively
            for (int _i = 0; _i < root.childCount; _i++)
            {
                Transform _child = root.GetChild(_i);

                // Add components found in the child
                _child.GetComponents(_components);

                // Recursive call for grandchildren
                if (_child.childCount > 0)
                {
                    _components.AddRange(_GetComponentsInRoot<T>(_child));
                }
            }

            return _components.ToArray();
        }


        public static string GetCurrentAnimation(this Animator animator)
        {
            return _GetCurrentAnimation(animator);
        }

        public static string _GetCurrentAnimation(Animator an)
        {
            if (an == null)
                return "";
            var _animatorinfo = an.GetCurrentAnimatorClipInfo(0);
            if (_animatorinfo.Length > 0)
                return _animatorinfo[0].clip.name;
            return "";
        }


        public static void ApplyConcreteDeserialization(this IDictionary<string, object> dict)
        {
            var keys = dict.Keys.ToList();

            foreach (var key in keys)
            {
                var value = dict[key];

                if (value is JObject jObj)
                {
                    // Deserialize the JObject into its concrete type
                    var concrete = ISerializer.TryDeserializeToConcrete(jObj);
                    dict[key] = concrete;

                    // If the result is another dictionary, recurse into it
                    if (concrete is IDictionary<string, object> childDict)
                        childDict.ApplyConcreteDeserialization();
                }
                else if (value is IDictionary<string, object> childDict)
                {
                    // If the value is already a nested dictionary, recurse into it
                    childDict.ApplyConcreteDeserialization();
                }

                // Finally, convert any long values to int
                var newValue = dict[key];
                if (newValue is long longVal)
                {
                    dict[key] = (int)(longVal % Int32.MaxValue);
                }
            }
        }

        public static string ToText<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary,
            string kvSeparator = ": ",
            string itemSeparator = "\n")
        {
            if (dictionary == null || dictionary.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var kvp in dictionary)
            {
                sb.Append(kvp.Key)
                    .Append(kvSeparator)
                    .Append(kvp.Value)
                    .Append(itemSeparator);
            }

            // Remove the trailing separator if present
            if (sb.Length >= itemSeparator.Length)
                sb.Length -= itemSeparator.Length;

            return sb.ToString();
        }

        public static void AddSave(this Dictionary<string, object> s, Dictionary<string, object> s2)
        {
            foreach (var _o in s2)
            {
                s[_o.Key] = _o.Value;
            }
        }

        public static void DrawDebugCone(Vector2 origin, Vector2 direction, float angle, float length, Color color)
        {
            direction.Normalize();

            // Calculate the two edge directions of the cone
            Vector2 rightEdge = Quaternion.Euler(0, 0, angle) * direction;
            Vector2 leftEdge = Quaternion.Euler(0, 0, -angle) * direction;

            // Draw center ray (optional)
            Debug.DrawRay(origin, direction * length, color);

            // Draw edges of the cone
            Debug.DrawRay(origin, rightEdge * length, color);
            Debug.DrawRay(origin, leftEdge * length, color);

            // Draw arc (optional for nice visualization)
            int segments = 10;
            for (int i = 0; i <= segments; i++)
            {
                float stepAngle = Mathf.Lerp(-angle, angle, i / (float)segments);
                Vector2 stepDir = Quaternion.Euler(0, 0, stepAngle) * direction;
                Debug.DrawRay(origin + stepDir * (length * 0.95f), stepDir * (length * 0.05f), color);
            }
        }
    }
}