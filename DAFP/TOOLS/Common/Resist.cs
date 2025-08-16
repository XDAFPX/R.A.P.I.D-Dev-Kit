namespace DAFP.TOOLS.Common
{
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