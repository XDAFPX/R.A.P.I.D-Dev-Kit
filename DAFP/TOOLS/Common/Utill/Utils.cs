using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BandoWare.GameplayTags;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Damage;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Thinkers;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using DAFP.TOOLS.ECS.ViewModel;
using Newtonsoft.Json.Linq;
using Optional;
using Optional.Unsafe;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.Common.Utill
{
    public static class Utils
    {
        public static Vector3 Randomize(this Vector3 vector3, float margin01)
        {
            vector3 += new Vector3(Random.Range(vector3.x * -margin01, vector3.x * margin01),
                Random.Range(vector3.y * -margin01, vector3.y * margin01),
                Random.Range(vector3.z * -margin01, vector3.z * margin01));
            return vector3;
        }

        public static uint Randomize(this uint value, float margin01)
        {
            value += Convert.ToUInt32(Random.Range(value * -margin01, value * margin01));
            return value;
        }

        public static float Randomize(this float value, float margin01)
        {
            value += Random.Range(value * -margin01, value * margin01);
            return value;
        }

        public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value)
        {
            foreach (var _kvp in dict)
                if (EqualityComparer<TValue>.Default.Equals(_kvp.Value, value))
                    return _kvp.Key;

            throw new Exception("Value not found in dictionary.");
        }

        public static Vector3 Add(this Vector3 v3, Vector2 v2)
        {
            return v3 + new Vector3(v2.x, v2.y, 0);
        }

        public static Vector3 Add(this Vector3 v3, IVectorBase v32)
        {
            return v3 + (Vector3)v32.TryGetVector3();
        }

        public static Vector3 Add(this Vector3 v3, Vector3 v32)
        {
            return v3 + v32;
        }

        public static Transform FindDeepChild(this Transform parent, string childName)
        {
            // First, check direct children
            var _result = parent.Find(childName);
            if (_result != null)
                return _result;

            // If not found, search recursively
            foreach (Transform _child in parent)
            {
                _result = _child.FindDeepChild(childName);
                if (_result != null)
                    return _result;
            }

            // Not found anywhere
            return null;
        }

        public static Vector3 ClampMinMagnitude(this Vector3 vector, float minLength)
        {
            var _sqrMagnitude = vector.sqrMagnitude;
            var _minLengthSqr = minLength * minLength;

            // If vector is already longer than minLength or it's zero, return as is
            if (_sqrMagnitude >= _minLengthSqr || _sqrMagnitude == 0f)
                return vector;

            var _magnitude = Mathf.Sqrt(_sqrMagnitude);
            var _scale = minLength / _magnitude;

            return new Vector3(vector.x * _scale, vector.y * _scale, vector.z * _scale);
        }

        public static Vector2 ClampMinMagnitude(this Vector2 vector, float minLength)
        {
            var _sqrMagnitude = vector.sqrMagnitude;
            var _minLengthSqr = minLength * minLength;

            // If vector is already longer than minLength or it's zero, return as is
            if (_sqrMagnitude >= _minLengthSqr || _sqrMagnitude == 0f)
                return vector;

            var _magnitude = Mathf.Sqrt(_sqrMagnitude);
            var _scale = minLength / _magnitude;

            return new Vector2(vector.x * _scale, vector.y * _scale);
        }


        public static List<AnimationClip> GetAllAnimationClips(this Animator animator)
        {
            return _GetAllAnimationClips(animator);
        }

        public static List<AnimationClip> _GetAllAnimationClips(Animator animator)
        {
            var _clips = new List<AnimationClip>();
            if (animator == null || animator.runtimeAnimatorController == null)
                return _clips;

            var _controller = animator.runtimeAnimatorController;

            // Check if it's an override controller
            if (_controller is AnimatorOverrideController _overrideController)
            {
                var _overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                _overrideController.GetOverrides(_overrides);

                foreach (var _pair in _overrides)
                    if (_pair.Value != null && !_clips.Contains(_pair.Value))
                        _clips.Add(_pair.Value);
            }
            else
            {
                // Default case
                _clips.AddRange(_controller.animationClips);
            }

            return _clips;
        }

        public static bool IsPointInCone(Vector2 origin, Vector2 direction, float angle, float length, Vector2 point)
        {
            var _toPoint = point - origin;

            // Outside range
            if (_toPoint.magnitude > length)
                return false;

            // Check angle
            var _angleToTarget = Vector2.Angle(direction.normalized, _toPoint.normalized);
            return _angleToTarget <= angle;
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
            var _components = new List<T>();

            // Add components on the root object itself
            root.GetComponents(_components);

            // Traverse all children recursively
            for (var _i = 0; _i < root.childCount; _i++)
            {
                var _child = root.GetChild(_i);

                // Add components found in the child
                _child.GetComponents(_components);

                // Recursive call for grandchildren
                if (_child.childCount > 0) _components.AddRange(_GetComponentsInRoot<T>(_child));
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

        public static bool TryDeInitialize(this IThinker thinker, IEntity host)
        {
            if (thinker.HasInitialized)
                thinker.Dispose(host);
            return thinker.HasInitialized;
        }


        public static bool TryInitialize(this IThinker thinker, IEntity host)
        {
            if (!thinker.HasInitialized)
                thinker.Initialize(host);
            return !thinker.HasInitialized;
        }

        public static void SwapBrains(this IEntity host, IThinker @new)
        {
            host.DeInitializeBrains(host.Brains);
            host.InitializeBrains(@new);
        }

        public static void ApplyConcreteDeserialization(this IDictionary<string, object> dict)
        {
            var _keys = dict.Keys.ToList();

            foreach (var _key in _keys)
            {
                var _value = dict[_key];

                if (_value is JObject _jObj)
                {
                    // Deserialize the JObject into its concrete type
                    var _concrete = ISerializer<ISavable>.TryDeserializeToConcrete(_jObj);
                    dict[_key] = _concrete;

                    // If the result is another dictionary, recurse into it
                    if (_concrete is IDictionary<string, object> _childDict)
                        _childDict.ApplyConcreteDeserialization();
                }
                else if (_value is IDictionary<string, object> _childDict)
                {
                    // If the value is already a nested dictionary, recurse into it
                    _childDict.ApplyConcreteDeserialization();
                }

                // Finally, convert any double values to float
                var _newValue1 = dict[_key];
                if (_newValue1 is double _double1) dict[_key] = Convert.ToSingle(_double1);

                // Finally, convert any long values to int
                var _newValue = dict[_key];
                if (_newValue is long _longVal) dict[_key] = (int)(_longVal % int.MaxValue);
            }
        }

        public static void ForEach<T>(this HashSet<T> set, Action<T> action)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var _item in set) action(_item);
        }

        public static string ToText<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary,
            string kvSeparator = ": ",
            string itemSeparator = "\n")
        {
            if (dictionary == null || dictionary.Count == 0)
                return string.Empty;

            var _sb = new StringBuilder();
            foreach (var _kvp in dictionary)
                _sb.Append(_kvp.Key)
                    .Append(kvSeparator)
                    .Append(_kvp.Value)
                    .Append(itemSeparator);

            // Remove the trailing separator if present
            if (_sb.Length >= itemSeparator.Length)
                _sb.Length -= itemSeparator.Length;

            return _sb.ToString();
        }

        public static void AddSave(this Dictionary<string, object> s, Dictionary<string, object> s2)
        {
            foreach (var _o in s2) s[_o.Key] = _o.Value;
        }

        public static void DrawDebugCone(Vector2 origin, Vector2 direction, float angle, float length, Color color)
        {
            direction.Normalize();

            // Calculate the two edge directions of the cone
            Vector2 _rightEdge = Quaternion.Euler(0, 0, angle) * direction;
            Vector2 _leftEdge = Quaternion.Euler(0, 0, -angle) * direction;

            // Draw center ray (optional)
            Debug.DrawRay(origin, direction * length, color);

            // Draw edges of the cone
            Debug.DrawRay(origin, _rightEdge * length, color);
            Debug.DrawRay(origin, _leftEdge * length, color);

            // Draw arc (optional for nice visualization)
            var _segments = 10;
            for (var _i = 0; _i <= _segments; _i++)
            {
                var _stepAngle = Mathf.Lerp(-angle, angle, _i / (float)_segments);
                Vector2 _stepDir = Quaternion.Euler(0, 0, _stepAngle) * direction;
                Debug.DrawRay(origin + _stepDir * (length * 0.95f), _stepDir * (length * 0.05f), color);
            }
        }

        public static void DrawDebugPosition(Vector3 position, float size = 0.1f, Color? color = null,
            float duration = 0f,
            bool depthTest = true)
        {
            var _drawColor = color ?? Color.white;
            // Principal axes
            Vector3[] _axes = { Vector3.right, Vector3.up, Vector3.forward };

            foreach (var _axis in _axes)
            {
                // Positive direction
                Debug.DrawRay(position, _axis * size, _drawColor, duration, depthTest);
                // Negative direction
                Debug.DrawRay(position, -_axis * size, _drawColor, duration, depthTest);
            }
        }


        public static IEnumerable<T> GetAllNodes<T>(this IEnumerable<T> nodes) where T : IOwnable<T>, IOwner<T>
        {
            HashSet<T> _all = new HashSet<T>();
            foreach (var _ownable in nodes)
            {
                _all.UnionWith(_ownable.GetAllNodes());
            }

            return _all;
        }

        public static IEnumerable<T> GetAllNodes<T>(this T node) where T : IOwnable<T>, IOwner<T>
        {
            if (node == null)
                yield break;

            var _visited = new HashSet<T>();
            var _stack = new Stack<T>();
            _stack.Push(node);

            while (_stack.Count > 0)
            {
                var _current = _stack.Pop();

                if (_current == null || !_visited.Add(_current))
                    continue;

                yield return _current;

                // Traverse children (pets)
                foreach (var _pet in _current.Pets.OfType<T>())
                {
                    if (!_visited.Contains(_pet))
                        _stack.Push(_pet);
                }
            }
        }

        public static IEnumerable<T> ToValues<T>(this IEnumerable<SerializableInterface<T>> arr) where T : class
        {
            return arr.Select((@interface => @interface.Value));
        }

        public static IEnumerable<IViewModel> Enabled(this IEnumerable<IViewModel> models)
        {
            return models.GetActiveViews();
        }

        public static IEnumerable<IViewModel> GetActiveViews(this IEnumerable<IViewModel> models)
        {
            return models.Where(viewModel => viewModel.Enabled);
        }

        public static void Do<T>(this IEnumerable<IViewModel> views, Action<T> action) where T : IViewModel
        {
            foreach (var _view in views.Enabled())
            {
                if (_view is T _typedView)
                {
                    action(_typedView);
                }
            }
        }

        public static Bounds Add(this Bounds a, Bounds b)
        {
            if (a.size == Vector3.zero) return b;
            if (b.size == Vector3.zero) return a;

            var _min = Vector3.Min(a.min, b.min);
            var _max = Vector3.Max(a.max, b.max);
            var _combined = new Bounds();
            _combined.SetMinMax(_min, _max);
            return _combined;
        }

        /// <summary>
        ///   Returns a new Bounds whose center is the difference of the two centers 
        ///   and whose size is the difference of the two sizes.
        /// </summary>
        public static Bounds Subtract(this Bounds a, Bounds b)
        {
            return new Bounds(a.center - b.center, a.size - b.size);
        }


        public static void DisableAll(this IEnumerable<ISwitchable> models)
        {
            foreach (var _switchable in models) _switchable.Disable();
        }

        public static void EnableAll(this IEnumerable<ISwitchable> models)
        {
            foreach (var _switchable in models) _switchable.Enable();
        }

        public static void Enable(this IEnumerable<DebugDrawLayer> models, string name)
        {
            var _find = models.FirstOrDefault(model => model.Name == name);
            if (_find == null)
                return;
            _find.Enable();
        }

        public static void Disable(this IEnumerable<DebugDrawLayer> models, string name)
        {
            var _find = models.FirstOrDefault(model => model.Name == name);
            if (_find == null)
                return;
            _find.Disable();
        }

        public static void SwitchTo<T>(this IEnumerable<IViewModel> models)
        {
            var _find = models.FirstOrDefault(model => model.GetType().IsSubclassOf(typeof(T)));
            if (_find == null)
                return;
            foreach (var _viewModel in models)
            {
                if (_viewModel == _find)
                    continue;
                _viewModel.Disable();
            }

            _find.Enable();
        }


        public static INameable FindByName(this IEnumerable<INameable> nameables, string name)
        {
            return nameables.FirstOrDefault(nameable => nameable.Name == name);
        }

        public static T DeepClone<T>(this T original) where T : ScriptableObject
        {
            var _clone = ScriptableObject.Instantiate(original);
            var _fields = original.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var _f in _fields)
            {
                var _v = _f.GetValue(original);

                // single reference
                if (_v is ScriptableObject _so)
                {
                    _f.SetValue(_clone, ScriptableObject.Instantiate(_so));
                    continue;
                }

                // array
                if (_v is Array _arr && _arr.Length > 0 && _arr.GetValue(0) is ScriptableObject)
                {
                    var _elementType = _arr.GetType().GetElementType();
                    var _newArr = Array.CreateInstance(_elementType, _arr.Length);

                    for (int _i = 0; _i < _arr.Length; _i++)
                        _newArr.SetValue(
                            _arr.GetValue(_i) is ScriptableObject _soElem
                                ? ScriptableObject.Instantiate(_soElem)
                                : _arr.GetValue(_i), _i);

                    _f.SetValue(_clone, _newArr);
                    continue;
                }

                // list
                if (_v is IList _list && _list.Count > 0 && _list[0] is ScriptableObject)
                {
                    var _newList = (IList)Activator.CreateInstance(_list.GetType());
                    foreach (var _item in _list)
                        _newList.Add(_item is ScriptableObject _soItem
                            ? ScriptableObject.Instantiate(_soItem)
                            : _item);
                    _f.SetValue(_clone, _newList);
                }
            }

            return _clone;
        }

        public static IInputController TryGetRootController(this BaseThinker thinker, Func<IInputController> fallback)
        {
            IInputController _controller = null;
            var _root = ((IOwnable<IThinker>)thinker).GetRootOwner();
            if (_root is IContainerOf<IInputController> _container)
            {
                _controller = _container.GetContents();
            }
            else
            {
                _controller = fallback.Invoke();
            }

            return _controller;
        }

        public static void DeepDestroy(this ScriptableObject instance)
        {
            if (instance == null) return;

            var _fields = instance.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var _f in _fields)
            {
                var _v = _f.GetValue(instance);

                if (_v is ScriptableObject _so)
                    ScriptableObject.Destroy(_so);

                else if (_v is Array _arr && _arr.Length > 0 && _arr.GetValue(0) is ScriptableObject)
                    foreach (var _obj1 in _arr)
                        if (_obj1 is ScriptableObject _soElem1)
                            ScriptableObject.Destroy(_soElem1);

                        else if (_v is IList _list && _list.Count > 0 && _list[0] is ScriptableObject)
                            foreach (var _obj in _list)
                                if (_obj is ScriptableObject _soElem)
                                    ScriptableObject.Destroy(_soElem);
            }

            ScriptableObject.Destroy(instance);
        }

        /// <summary>
        /// Calculates the combined bounds of all colliders (2D or 3D) on the given GameObject and its children.
        /// </summary>
        public static Bounds CalculateCombinedBounds(GameObject root)
        {
            // --- 3D Colliders ---
            var _colliders3D = root.GetComponentsInChildren<Collider>();
            // --- 2D Colliders ---
            var _colliders2D = root.GetComponentsInChildren<Collider2D>();

            if (_colliders3D.Length == 0 && _colliders2D.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

            var _initialized = false;
            var _combined = new Bounds(Vector3.zero, Vector3.zero);

            // --- Handle 3D Colliders ---
            foreach (var _col in _colliders3D)
            {
                var _localBounds = new Bounds(
                    root.transform.InverseTransformPoint(_col.bounds.center),
                    _col.bounds.size
                );

                if (!_initialized)
                {
                    _combined = _localBounds;
                    _initialized = true;
                }
                else
                {
                    _combined.Encapsulate(_localBounds);
                }
            }

            // --- Handle 2D Colliders ---
            foreach (var _col in _colliders2D)
            {
                var _localBounds = new Bounds(
                    root.transform.InverseTransformPoint(_col.bounds.center),
                    _col.bounds.size
                );

                if (!_initialized)
                {
                    _combined = _localBounds;
                    _initialized = true;
                }
                else
                {
                    _combined.Encapsulate(_localBounds);
                }
            }

            return _combined;
        }

        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            var _component = gameObject.GetComponent<T>();
            if (_component == null)
                _component = gameObject.AddComponent<T>();

            return _component;
        }

        public static T AddOrGetComponent<T>(this Transform transform) where T : Component
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return transform.gameObject.AddOrGetComponent<T>();
        }

        public static IEnumerable<T> FilterThrough<T>(this IEnumerable<T> arr, IFilter<T> filter)
        {
            return filter.Filter(arr);
        }

        public static IEnumerable<IEntity> FilterThrough(this IEntity[] arr, EntityFilter filter)
        {
            return filter.Filter(arr);
        }

        public static Vector3 LookVector(this IEntity arr)
        {
            return arr.EyeVector.Normalized.TryGetVector3();
        }


        public static void X(this IEntity e, float x)
        {
            var _t = e.GetWorldRepresentation().transform;
            _t.position = new Vector3(x, _t.position.y, _t.position.z);
        }

        public static float X(this IEntity e)
        {
            return e.GetWorldRepresentation().transform.position.x;
        }

        public static void Y(this IEntity e, float y)
        {
            var _t = e.GetWorldRepresentation().transform;
            _t.position = new Vector3(_t.position.x, y, _t.position.z);
        }

        public static float Y(this IEntity e)
        {
            return e.GetWorldRepresentation().transform.position.y;
        }

        public static void Z(this IEntity e, float z)
        {
            var _t = e.GetWorldRepresentation().transform;
            _t.position = new Vector3(_t.position.x, _t.position.y, z);
        }

        public static float Z(this IEntity e)
        {
            return e.GetWorldRepresentation().transform.position.z;
        }

        public static Vector3 Pos(this IEntity e)
        {
            return e.GetWorldRepresentation().transform.position;
        }

        public static void Pos(this IEntity e, Vector3 pos)
        {
            e.GetWorldRepresentation().transform.position = pos;
        }

        // -----------------------------
        // ROTATION
        // -----------------------------

        public static Quaternion Rot(this IEntity e)
        {
            return e.GetWorldRepresentation().transform.rotation;
        }

        public static void Rot(this IEntity e, Quaternion rot)
        {
            e.GetWorldRepresentation().transform.rotation = rot;
        }

        // Rotation using Euler angles

        public static Vector3 Euler(this IEntity e)
        {
            return e.GetWorldRepresentation().transform.eulerAngles;
        }

        public static void Euler(this IEntity e, Vector3 euler)
        {
            e.GetWorldRepresentation().transform.eulerAngles = euler;
        }

        public static bool TryGetValue<T>(this Option<T> option, out T val)
        {
            val = default;
            if (!option.HasValue) return false;
            val = option.ValueOrFailure();
            return true;
        }

        public static bool IsInBounds<T>(this IReadOnlyList<T> collection, int index)
        {
            return collection != null && index >= 0 && index < collection.Count;
        }

        /// <summary>
        /// Overload for arrays (for convenience).
        /// </summary>
        public static bool IsInBounds<T>(this T[] array, int index)
        {
            return array != null && index >= 0 && index < array.Length;
        }

        // public static IStat<T> PegOrDefault<T>(this WhiteBoard<T> board, string name, IStat<T> @default)
        // {
        //     var _val = board.Pets.Cast<IPeg<T>>().FindByName(name);
        //     return _val != default ? ((IPeg<T>)_val) : @default;
        // }

        public static IEnumerable<T> Filter<T>(this IFilter<T> filter, IEnumerable<T> ents)
        {
            foreach (var _entity in ents)
            {
                if (filter.Evaluate(_entity))
                    yield return _entity;
            }
        }

        public static float ComputeInterceptTime(Vector3 p, Vector3 T, Vector3 v, float s)
        {
            Vector3 _diff = T - p;
            float _a = v.sqrMagnitude - s * s;
            float _b = 2f * Vector3.Dot(_diff, v);
            float _c = _diff.sqrMagnitude;

            // If a is zero, fallback to linear solution
            if (Mathf.Abs(_a) < 0.0001f)
                return -_c / _b;

            float _discriminant = _b * _b - 4f * _a * _c;

            // No real solution → cannot intercept
            if (_discriminant < 0)
                return -1;

            float _sqrt = Mathf.Sqrt(_discriminant);
            float _t1 = (-_b + _sqrt) / (2f * _a);
            float _t2 = (-_b - _sqrt) / (2f * _a);

            // we need the smallest *positive* time
            if (_t1 > 0 && _t2 > 0) return Mathf.Min(_t1, _t2);
            if (_t1 > 0) return _t1;
            return _t2; // might be negative (invalid)
        }

        public static T Clamp<T>(T value, T min, T max, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            if (comparer.Compare(min, max) > 0)
                throw new ArgumentException("min must be less than or equal to max");

            if (comparer.Compare(value, min) < 0)
                return min;

            if (comparer.Compare(value, max) > 0)
                return max;

            return value;
        }

        public static IDamage Construct(this IDamageBoard dmg, Option<IVectorBase> vec)
        {
            return dmg.Construct(Option.None<IEntity>(), vec, dmg);
        }

        public static IDamage Construct(this IDamageBoard dmg, Option<IEntity> ent, Option<IVectorBase> vec)
        {
            return dmg.Construct(ent, vec, dmg);
        }

        public static IDamage Construct(this IDamageBoard dmg, Option<IVectorBase> vec, IStat<uint> stat)
        {
            return dmg.Construct(Option.None<IEntity>(), vec, stat);
        }

        public static uint SafeSubtract(this uint value, uint amount)
        {
            return value > amount ? value - amount : 0u;
        }

        /// <summary>
        /// Safely decrements a uint by 1, clamping at 0.
        /// </summary>
        public static uint SafeDecrement(this uint value)
        {
            return value > 0 ? value - 1 : 0u;
        }

        /// <summary>
        /// Tries to subtract from a uint, returns true if successful (no underflow).
        /// </summary>
        public static bool TrySubtract(this uint value, uint amount, out uint result)
        {
            if (value >= amount)
            {
                result = value - amount;
                return true;
            }

            result = 0u;
            return false;
        }

        public static IHaveGameplayTag AddGTag(this IHaveGameplayTag a, IHaveGameplayTag tag)
        {
            a.GameplayTag.AddTags(tag.GameplayTag);
            return a;
        }

        public static IHaveGameplayTag RemoveGTag(this IHaveGameplayTag a, IHaveGameplayTag tag)
        {
            a.GameplayTag.RemoveTags(tag.GameplayTag);
            return a;
        }

        public static IHaveGameplayTag AddGTag(this IHaveGameplayTag a, GameplayTagContainer tag)
        {
            a.GameplayTag.AddTags(tag);
            return a;
        }

        public static IHaveGameplayTag RemoveGTag(this IHaveGameplayTag a, GameplayTagContainer tag)
        {
            a.GameplayTag.RemoveTags(tag);
            return a;
        }

        public static IHaveGameplayTag AddGTag(this IHaveGameplayTag a, GameplayTag tag)
        {
            a.GameplayTag.Add(tag);
            return a;
        }

        public static IHaveGameplayTag RemoveGTag(this IHaveGameplayTag a, GameplayTag tag)
        {
            a.GameplayTag.RemoveTag(tag);
            return a;
        }

        public static float GetRatio(this IStat<int> a)
        {
            return (float)a.Value / (float)a.MaxValue;
        }

        public static float GetRatio(this IStat<float> a)
        {
            return (float)a.Value / (float)a.MaxValue;
        }

        public static float GetRatio(this IStat<uint> a)
        {
            return (float)a.Value / (float)a.MaxValue;
        }

        public static bool Alive(this IStat<uint> stat)
        {
            return !stat.Dead();
        }

        public static T TakeDamage<T>(this T stat,IDamage damage) where T : IStat<uint>
        {
            stat.Value -= damage.Info.Damage.Value;
            return stat;
        }

        public static bool Dead(this IStat<uint> stat)
        {
            return stat.Value == 0;
        }
    }
}