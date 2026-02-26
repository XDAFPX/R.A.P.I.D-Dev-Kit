using System.Runtime.CompilerServices;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.GAME.Assets;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.ViewModel;
using RapidLib.DAFP.TOOLS.Aspects;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EntVisualEffect : Entity, IGamePoolable<EntVisualEffect>, ISwitchable
    {
        [SerializeField] private string uName;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private bool DoTick;

        public override NonEmptyList<IViewModel> SetupView()
        {
            return GetDefaultView();
        }

        public override ITicker<IEntity> EntityTicker => DoTick ? realTicker : World.EmptyTicker;

        [Inject(Id = "DefaultEffectsEntityGameplayTicker")]
        private ITicker<IEntity> realTicker;

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
            StartEffect();
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
        [EntNotifyAspect]
        public void OnSpawn()
        {
            StartEffect();
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
            // base.OnDispose();
            AssetManager.ReleaseIGamePoolable(this);
        }

        public override void Remove(EntityRemovalReason removalReason)
        {
            OnDispose();
        }
    }
}