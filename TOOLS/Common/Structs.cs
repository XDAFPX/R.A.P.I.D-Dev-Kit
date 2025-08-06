using DAFP.TOOLS.ECS;
using UnityEngine;

namespace DAFP.TOOLS.Common
{
    public struct Damage
    {
        public float Interaction;
        public DamageType[] Types;
        public Entity Author;
        public Vector2 Direction;
        public Damage(float interaction, DamageType[] types, Entity author,Vector2 dir)
        {
            Interaction = interaction;
            Types = types;
            this.Author = author;
            Direction = dir;
        }
        public Damage(float interaction ,DamageType[] types,Entity author)
        {
            Interaction = interaction;
            Types = types;
            this.Author = author;
            Direction = Vector2.zero;
        }
        public Damage(float interaction, DamageType type, Entity author)
        {
            Direction = Vector2.zero;
            Interaction = interaction;
            Types = new DamageType[] {type};
            this.Author = author;
        }
        public Damage(float interaction, DamageType type, Entity author,Vector2 dir)
        {
            Direction = dir;
            Interaction = interaction;
            Types = new DamageType[] { type };
            this.Author = author;
        }
    }
    [System.Serializable]
    public struct Resist
    {
        public float Multi;
        public DamageType Type;
    
        public Resist(float multi, DamageType type)
        {
            Multi = multi;
            Type = type;
        }
    }
}