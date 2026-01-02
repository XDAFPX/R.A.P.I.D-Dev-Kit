using System;
using UnityEngine;

namespace DAFP.TOOLS.Common.Maths
{
    // Generic vector interface supporting common operations for any dimension
    public interface IVectorBase
    {
        float? GetValueAtDimension(int dimension);
        void SetValueAtDimension(int dimension, float? value);
        int Dimensions { get; }
        float Magnitude { get; }
        IVectorBase Normalized { get; }
        IVectorBase Normalize();

        IVectorBase Scale(float scalar);

        IVectorBase Reverse()
        {
            return Scale(-1);
        }

        V2 TryGetVector2()
        {
            var x = GetValueAtDimension(1) ?? 0f;
            var y = GetValueAtDimension(2) ?? 0f;
            return new V2(x, y);
        }

        V3 TryGetVector3()
        {
            var x = GetValueAtDimension(1) ?? 0f;
            var y = GetValueAtDimension(2) ?? 0f;
            var z = GetValueAtDimension(3) ?? 0f;
            return new V3(x, y, z);
        }

        V4 TryGetVector4()
        {
            var x = GetValueAtDimension(1) ?? 0f;
            var y = GetValueAtDimension(2) ?? 0f;
            var z = GetValueAtDimension(3) ?? 0f;
            var w = GetValueAtDimension(4) ?? 0f;
            return new V4(x, y, z, w);
        }
    }

    public interface IVector<TSelf> : IVectorBase where TSelf : struct, IVector<TSelf>
    {
        IVectorBase IVectorBase.Normalize()
        {
            return Normalized;
        }

        IVectorBase IVectorBase.Normalized => Normalized;

        TSelf Normalized { get; }

        TSelf Add(TSelf other);
        TSelf Subtract(TSelf other);
        TSelf Scale(float scalar);

        IVectorBase IVectorBase.Scale(float scalar)
        {
            return Scale(scalar);
        }

        float Dot(TSelf other);

        IVector<TSelf> TrySetVector2(V2 vec)
        {
            SetValueAtDimension(1, vec.x);
            SetValueAtDimension(2, vec.y);
            return this;
        }

        IVector<TSelf> TrySetVector3(V3 vec)
        {
            SetValueAtDimension(1, vec.x);
            SetValueAtDimension(2, vec.y);
            SetValueAtDimension(3, vec.z);
            return this;
        }

        IVector<TSelf> TrySetVector4(V4 vec)
        {
            SetValueAtDimension(1, vec.x);
            SetValueAtDimension(2, vec.y);
            SetValueAtDimension(3, vec.z);
            SetValueAtDimension(4, vec.w);
            return this;
        }
    }

    public interface IVector2<TSelf> : IVector<TSelf> where TSelf : struct, IVector2<TSelf>
    {
        float x { get; set; }
        float y { get; set; }
    }

    public interface IVector3<TSelf> : IVector<TSelf> where TSelf : struct, IVector3<TSelf>
    {
        float x { get; set; }
        float y { get; set; }
        float z { get; set; }
    }

    public interface IVector4<TSelf> : IVector<TSelf> where TSelf : struct, IVector4<TSelf>
    {
        float x { get; set; }
        float y { get; set; }
        float z { get; set; }
        float w { get; set; }
    }

//--------------------------------------
// Vector 2
//--------------------------------------
    [Serializable]
    public struct V2 : IVector2<V2>
    {
        [field: SerializeField] public float x { get; set; }
        [field: SerializeField] public float y { get; set; }

        public V2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(V2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator V2(Vector2 v)
        {
            return new V2(v.x, v.y);
        }

        public static implicit operator Vector3(V2 v)
        {
            return new Vector3(v.x, v.y, 0f);
        }

        public static implicit operator V2(Vector3 v)
        {
            return new V2(v.x, v.y);
        }

        public void SetValueAtDimension(int dimension, float? value)
        {
            if (value == null) return;
            switch (dimension)
            {
                case 1: x = value.Value; break;
                case 2: y = value.Value; break;
            }
        }

        public float? GetValueAtDimension(int dimension)
        {
            return dimension switch
            {
                1 => x,
                2 => y,
                _ => null
            };
        }

        public int Dimensions => 2;
        public float Magnitude => Mathf.Sqrt(x * x + y * y);
        public V2 Normalized => Magnitude < Mathf.Epsilon ? new V2(0, 0) : new V2(x / Magnitude, y / Magnitude);

        public V2 Add(V2 other)
        {
            return new V2(x + other.x, y + other.y);
        }

        public V2 Subtract(V2 other)
        {
            return new V2(x - other.x, y - other.y);
        }

        public V2 Scale(float scalar)
        {
            return new V2(x * scalar, y * scalar);
        }


        public float Dot(V2 other)
        {
            return x * other.x + y * other.y;
        }

        public static V2 operator +(V2 a, V2 b)
        {
            return a.Add(b);
        }

        public static V2 operator -(V2 a, V2 b)
        {
            return a.Subtract(b);
        }

        public static V2 operator *(V2 a, float s)
        {
            return a.Scale(s);
        }

        public static V2 operator *(float s, V2 a)
        {
            return a.Scale(s);
        }

        public override string ToString()
        {
            return $"x: {x} , y: {y}";
        }
    }

//--------------------------------------
// Vector 3
//--------------------------------------
    [Serializable]
    public struct V3 : IVector3<V3>
    {
        [field: SerializeField] public float x { get; set; }
        [field: SerializeField] public float y { get; set; }
        [field: SerializeField] public float z { get; set; }

        public V3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vector3(V3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator V3(Vector3 v)
        {
            return new V3(v.x, v.y, v.z);
        }

        public void SetValueAtDimension(int dimension, float? value)
        {
            if (value == null) return;
            switch (dimension)
            {
                case 1: x = value.Value; break;
                case 2: y = value.Value; break;
                case 3: z = value.Value; break;
            }
        }

        public float? GetValueAtDimension(int dimension)
        {
            return dimension switch
            {
                1 => x,
                2 => y,
                3 => z,
                _ => null
            };
        }

        public int Dimensions => 3;
        public float Magnitude => Mathf.Sqrt(x * x + y * y + z * z);

        public V3 Normalized => Magnitude < Mathf.Epsilon
            ? new V3(0, 0, 0)
            : new V3(x / Magnitude, y / Magnitude, z / Magnitude);

        public V3 Add(V3 other)
        {
            return new V3(x + other.x, y + other.y, z + other.z);
        }

        public V3 Subtract(V3 other)
        {
            return new V3(x - other.x, y - other.y, z - other.z);
        }

        public V3 Scale(float scalar)
        {
            return new V3(x * scalar, y * scalar, z * scalar);
        }

        public float Dot(V3 other)
        {
            return x * other.x + y * other.y + z * other.z;
        }

        public static V3 operator +(V3 a, V3 b)
        {
            return a.Add(b);
        }

        public static V3 operator -(V3 a, V3 b)
        {
            return a.Subtract(b);
        }

        public static V3 operator *(V3 a, float s)
        {
            return a.Scale(s);
        }

        public static V3 operator *(float s, V3 a)
        {
            return a.Scale(s);
        }

        public override string ToString()
        {
            return $"x: {x} , y: {y} , z: {z}";
        }
    }

//--------------------------------------
// Vector 4
//--------------------------------------
    [Serializable]
    public struct V4 : IVector4<V4>
    {
        [field: SerializeField] public float x { get; set; }
        [field: SerializeField] public float y { get; set; }
        [field: SerializeField] public float z { get; set; }
        [field: SerializeField] public float w { get; set; }

        public V4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator Vector4(V4 v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }

        public static implicit operator V4(Vector4 v)
        {
            return new V4(v.x, v.y, v.z, v.w);
        }

        public void SetValueAtDimension(int dimension, float? value)
        {
            if (value == null) return;
            switch (dimension)
            {
                case 1: x = value.Value; break;
                case 2: y = value.Value; break;
                case 3: z = value.Value; break;
                case 4: w = value.Value; break;
            }
        }

        public float? GetValueAtDimension(int dimension)
        {
            return dimension switch
            {
                1 => x,
                2 => y,
                3 => z,
                4 => w,
                _ => null
            };
        }

        public int Dimensions => 4;
        public float Magnitude => Mathf.Sqrt(x * x + y * y + z * z + w * w);

        public V4 Normalized => Magnitude < Mathf.Epsilon
            ? new V4(0, 0, 0, 0)
            : new V4(x / Magnitude, y / Magnitude, z / Magnitude, w / Magnitude);

        public V4 Add(V4 other)
        {
            return new V4(x + other.x, y + other.y, z + other.z, w + other.w);
        }

        public V4 Subtract(V4 other)
        {
            return new V4(x - other.x, y - other.y, z - other.z, w - other.w);
        }

        public V4 Scale(float scalar)
        {
            return new V4(x * scalar, y * scalar, z * scalar, w * scalar);
        }

        public float Dot(V4 other)
        {
            return x * other.x + y * other.y + z * other.z + w * other.w;
        }

        public static V4 operator +(V4 a, V4 b)
        {
            return a.Add(b);
        }

        public static V4 operator -(V4 a, V4 b)
        {
            return a.Subtract(b);
        }

        public static V4 operator *(V4 a, float s)
        {
            return a.Scale(s);
        }

        public static V4 operator *(float s, V4 a)
        {
            return a.Scale(s);
        }

        public override string ToString()
        {
            return $"x: {x} , y: {y} , z: {z} , w: {w}";
        }
    }
}