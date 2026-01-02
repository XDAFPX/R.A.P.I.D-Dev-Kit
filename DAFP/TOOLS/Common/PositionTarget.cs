using UnityEngine;

namespace DAFP.TOOLS.Common
{
    public readonly struct PositionTarget
    {
        private readonly Vector3 _vector;
        private readonly Transform _transform;
        public bool IsVector3 { get; }
        public bool IsTransform => !IsVector3;

        private PositionTarget(Vector3 v)
        {
            _vector = v;
            _transform = null;
            IsVector3 = true;
        }

        private PositionTarget(Transform t)
        {
            _transform = t;
            _vector = default;
            IsVector3 = false;
        }

        public Vector3 GetAnyValidPosition()
        {
            if (IsVector3)
                return Vector3;
            if (IsTransform && _transform != null)
                return Transform.position;
            if (_vector != default)
                return _vector;
            return default;
        }

        public Vector3 Vector3
        {
            get
            {
                if (!IsVector3)
                    throw new System.InvalidOperationException("Not a Vector3 result");
                return _vector;
            }
        }

        public Transform Transform
        {
            get
            {
                if (IsVector3)
                    throw new System.InvalidOperationException("Not a Transform result");
                return _transform;
            }
        }

        // 1. Try to get the Vector3 if this target is a Vector3.
        public bool TryGetVector3(out Vector3 v)
        {
            if (IsVector3)
            {
                v = _vector;
                return true;
            }

            v = default;
            return false;
        }

        // 2. Try to get the Transform if this target is a Transform and not null.
        public bool TryGetTransform(out Transform t)
        {
            if (!IsVector3 && _transform != null)
            {
                t = _transform;
                return true;
            }

            t = null;
            return false;
        }

        public static PositionTarget FromVector3(Vector3 v)
        {
            return new PositionTarget(v);
        }

        public static PositionTarget FromTransform(Transform t)
        {
            return new PositionTarget(t);
        }
    }
}