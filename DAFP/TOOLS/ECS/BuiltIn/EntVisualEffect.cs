using System.Runtime.CompilerServices;
using DAFP.GAME.Assets;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.ViewModel;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EntVisualEffect : Entity, IGameGamePoolable<EntVisualEffect>, ISwitchable
    {
        [SerializeField] private string uName;

        public override NonEmptyList<IViewModel> SetupView()
        {
            return GetDefaultView();
        }

        public override ITicker<IEntity> EntityTicker => Services.World.EMPTY_TICKER;

        protected ParticleSystem ParticleSystem;
        protected Animator Animator;

        public void ToggleEffect(bool toggle)
        {
            if (toggle)
                StartEffect();
            else
                StopEffect();
        }

        public virtual void StopEffect()
        {
            if (ParticleSystem != null)
            {
                ParticleSystem.Stop();
            }

            if (Animator != null)
            {
                Animator.StopPlayback();
            }
        }

        public virtual void StartEffect()
        {
            if (ParticleSystem != null)
            {
                ParticleSystem.Play();
            }

            if (Animator != null)
            {
                Animator.StartPlayback();
            }
        }

        protected override void InitializeInternal()
        {
            if (TryGetComponent(out ParticleSystem _system))
                ParticleSystem = _system;
            if (TryGetComponent(out Animator _animator))
                Animator = _animator;
        }

        protected override void TickInternal()
        {
        }

        public string UName => uName;
        public string Prefix => "Effects";

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