﻿using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class Prop : Entity
    {
        public override NonEmptyList<IViewModel> SetupView()
        {
            var Render = new RendererView();
            Render.Construct(this);
            return new NonEmptyList<IViewModel>(Render);
        }

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