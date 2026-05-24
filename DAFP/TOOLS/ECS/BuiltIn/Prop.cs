using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class Prop : Entity
    {
        public override IEnumerable<IViewModel> SetupView()
        {
            return new EmptyView().ToEnumerable();
        }

        public override ITicker EntityTicker => World.EmptyTicker;

        protected override void TickInternal()
        {
        }

        protected override void InitializeInternal()
        {
        }
    }
}