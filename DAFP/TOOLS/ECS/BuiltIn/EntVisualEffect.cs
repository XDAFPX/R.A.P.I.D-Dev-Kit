using System.Runtime.CompilerServices;
using DAFP.GAME.Assets;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.ViewModel;
using UnityEngine;
using UnityEngine.TestTools;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EntVisualEffect : Entity, IGameGamePoolable<EntVisualEffect>
    {
        [SerializeField] private string uName;

        public override NonEmptyList<IViewModel> SetupView()
        {
            return GetDefaultView();
        }

        public override ITicker<IEntity> EntityTicker => Services.World.EMPTY_TICKER;

        protected override void InitializeInternal()
        {
        }

        protected override void TickInternal()
        {
        }

        public string UName => uName;

        public Component Self()
        {
            return this;
        }

        public void OnSpawn()
        {
        }


        public virtual EntVisualEffect ResetObj()
        {
            return this;
        }

        public virtual EntVisualEffect Get()
        {
            return this;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            AssetManager.ReleaseIGamePoolable(this);
        }
    }
}