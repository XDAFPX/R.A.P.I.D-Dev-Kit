using System;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using UnityEngine;

namespace DAFP.TOOLS.Common
{
    [System.Serializable]
    public struct Damage : IComparable, IComparable<Damage>
    {
        [Min(0)] public float Interaction;
        public DamageType[] Types;
        public IEntity Author;
        [HideInInspector] public Vector3 Direction;

        public Damage(float interaction, DamageType[] type, IEntity author, Vector3 dir)
        {
            Direction = dir;
            Interaction = Mathf.Max(interaction, 0);
            Types = type;
            this.Author = author;
        }

        public Damage(float interaction, DamageType type, IEntity author, Vector3 dir)
        {
            Direction = dir;
            Interaction = Mathf.Max(interaction, 0);
            Types = new DamageType[] { type };
            this.Author = author;
        }

        public Damage(float interaction, Damage old)
        {
            Interaction = Mathf.Max(interaction, 0);
            Types = old.Types;
            Direction = old.Direction;
            Author = old.Author;
        }

        // Implicitly convert Damage → float (returns Interaction)
        public static implicit operator float(Damage dmg)
        {
            return dmg.Interaction;
        }

        // Optionally, also allow conversion to double
        public static implicit operator double(Damage dmg)
        {
            return dmg.Interaction;
        }

// Damage op Damage
        public static Damage operator +(Damage a, Damage b) // step 2
            => new Damage(a.Interaction + b.Interaction, a.Types, a.Author, a.Direction);

        public static Damage operator -(Damage a, Damage b) // step 3
            => new Damage(a.Interaction - b.Interaction, a.Types, a.Author, a.Direction);

        public static Damage operator *(Damage a, Damage b) // step 4
            => new Damage(a.Interaction * b.Interaction, a.Types, a.Author, a.Direction);

        public static Damage operator /(Damage a, Damage b) // step 5
            => new Damage(a.Interaction / (b.Interaction == 0 ? Mathf.Epsilon : b.Interaction), a.Types, a.Author,
                a.Direction);

        // Damage op float
        public static Damage operator +(Damage dmg, float scalar) // step 6
            => new Damage(dmg.Interaction + scalar, dmg.Types, dmg.Author, dmg.Direction);

        public static Damage operator -(Damage dmg, float scalar)
            => new Damage(dmg.Interaction - scalar, dmg.Types, dmg.Author, dmg.Direction);

        public static Damage operator *(Damage dmg, float scalar)
            => new Damage(dmg.Interaction * scalar, dmg.Types, dmg.Author, dmg.Direction);

        public static Damage operator /(Damage dmg, float scalar)
            => new Damage(dmg.Interaction / (scalar == 0 ? Mathf.Epsilon : scalar), dmg.Types, dmg.Author,
                dmg.Direction);

        // float op Damage
        public static Damage operator +(float scalar, Damage dmg)
            => new Damage(scalar + dmg.Interaction, dmg.Types, dmg.Author, dmg.Direction);

        public static Damage operator -(float scalar, Damage dmg)
            => new Damage(scalar - dmg.Interaction, dmg.Types, dmg.Author, dmg.Direction);

        public static Damage operator *(float scalar, Damage dmg)
            => new Damage(scalar * dmg.Interaction, dmg.Types, dmg.Author, dmg.Direction);

        public static Damage operator /(float scalar, Damage dmg)
            => new Damage(scalar / (dmg.Interaction == 0 ? Mathf.Epsilon : dmg.Interaction), dmg.Types, dmg.Author,
                dmg.Direction);

        public static Damage Clamp(Damage value, Damage min, Damage max)
        {
            return new Damage(Mathf.Clamp(value, min, max), value.Types, value.Author, value.Direction);
        }


        public Damage Randomize(float margin01)
        {
            return new Damage(Interaction.Randomize(margin01), this);
        }

        public int CompareTo(Damage other)
        {
            return Interaction.CompareTo(other.Interaction);
        }

        /// <summary>
        /// Non-generic IComparable implementation.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is Damage other)
                return CompareTo(other);
            throw new ArgumentException("Object is not of type Damage", nameof(obj));
        }
    }
}