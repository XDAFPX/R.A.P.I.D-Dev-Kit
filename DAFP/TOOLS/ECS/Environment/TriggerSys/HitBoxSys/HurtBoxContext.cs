namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    [System.Serializable]
    public struct HurtBoxContext<T>
    {
        public HurtBox<T> FlaggedBox;
        public HurtGroup<T> Group;

        public HurtBoxContext(HurtBox<T> flaggedBox,   HurtGroup<T> group)
        {
            FlaggedBox = flaggedBox;
            Group = group;
        }
    }
}