using DAFP.TOOLS.ECS.Services;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class Prop : Entity
    {
        [Inject(Id = "DefaultUpdateEntityGameplayTicker")]
        public override ITicker<IEntity> EntityTicker { get; }
        protected override void TickInternal()
        {
        }

        protected override void InitializeInternal()
        {
        }
    }
}