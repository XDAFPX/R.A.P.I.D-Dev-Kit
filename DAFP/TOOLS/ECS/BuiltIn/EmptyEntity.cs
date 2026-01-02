using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.ViewModel;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EmptyEntity : Entity
    {
        public override NonEmptyList<IViewModel> SetupView()
        {
            return new NonEmptyList<IViewModel>(new EmptyView());
        }

        public override ITicker<IEntity> EntityTicker { get; } = Services.World.EMPTY_TICKER;
        
        protected override void InitializeInternal()
        {
        }

        protected override void TickInternal()
        {
        }
    }
}