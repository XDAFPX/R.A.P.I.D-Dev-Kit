namespace DAFP.TOOLS.ECS
{
    public abstract class EntityRemovalReason
    {
        public static readonly DefaultEntityRemovalReason DEFAULT = new();
        public static readonly KilledEntityRemovalReason KILLED = new();
        public static readonly VisualEntityRemovalReason VISUAL = new();
        public bool Destroy { get; }
        public bool Immediately { get; }

        protected EntityRemovalReason(bool destroy, bool immediate)
        {
            Immediately = immediate;
            Destroy = destroy;
        }

        public abstract void OnBeforeRemove(Entity entity);


        public class DefaultEntityRemovalReason : EntityRemovalReason
        {
            public DefaultEntityRemovalReason() : base(destroy: true, false)
            {
            }

            public DefaultEntityRemovalReason(bool destroy, bool immediate) : base(destroy, immediate)
            {
            }

            public override void OnBeforeRemove(Entity entity)
            {
            }
        }

        public class VisualEntityRemovalReason : EntityRemovalReason
        {
            public VisualEntityRemovalReason() : base(false, false)
            {
            }

            public override void OnBeforeRemove(Entity entity)
            {
            }
        }

        public class KilledEntityRemovalReason : EntityRemovalReason
        {
            public KilledEntityRemovalReason() : base(true, false)
            {
            }

            public override void OnBeforeRemove(Entity entity)
            {
            }
        }
    }
}