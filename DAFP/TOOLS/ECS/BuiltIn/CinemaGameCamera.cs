using System.Collections.Generic;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.ViewModel;
using DAFP.TOOLS.Injection;
using ModestTree;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;

#if CINEMAMACHINE
namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(CinemachineBrain))]
    public class CinemaGameCamera : EmptyEntity, IGameCamera
    {
        [Inject] private ICameraManager cameraManager;
        [GetComponent] private CinemachineBrain brain;

        public List<ICameraManager> Owners { get; } = new();
        [DeclareStat("Zoom", 10f)] public IStat<float> Zoom { get; }

        // Initializing list to prevent NullReferenceException on first Shake()
        protected List<ShakeInstance> Instances = new();

        // Accumulator variables to gather the multi-shake contributions
        private Vector3 _combinedPositionOffset;
        private Vector3 _combinedRotationOffset;

        public override IEnumerable<IViewModel> SetupView()
        {
            return new EmptyView().ToEnumerable();
        }

        public override ITicker EntityTicker => World.DefaultUpdate;

        protected override void InitializeInternal()
        {
            ((IGameCamera)this).ChangeOwner(cameraManager);
            set_zoom(Zoom.Value);
            Zoom.OnUpdateValue += (stat, pvalue) =>
            {
                var val = stat.Value;
                set_zoom(val);
            };
        }

        private void set_zoom(float val)
        {
            if (brain.ActiveVirtualCamera is CinemachineCamera cam)
            {
                if (cam.TryGetComponent(out CinemachinePositionComposer composer))
                {
                    composer.CameraDistance = val;
                    return;
                }

                if (cam.Lens.Orthographic)
                    cam.Lens.OrthographicSize = val;
                else
                    cam.Lens.FieldOfView = val;
            }
        }

        protected override void TickInternal()
        {
            ProcessShakes();
        }

        protected virtual void ProcessShakes()
        {
            // Reset offsets every frame before recalculating active shake states
            _combinedPositionOffset = Vector3.zero;
            _combinedRotationOffset = Vector3.zero;

            if (Instances.IsEmpty())
                return;

            // Iterate backward through the primary list to allow safe removal while tracking state
            for (int i = Instances.Count - 1; i >= 0; i--)
            {
                var _shake = Instances[i];

                // Tick the timer. If it returns true/completes, strip it out of active calculations
                if (_shake.Timer.tryCompleteTimer(EntityTicker.DeltaTime))
                {
                    Instances.RemoveAt(i);
                    continue;
                }

                // Process single instance displacement calculations
                ProcessShake(_shake);
            }

            // Apply total calculated offsets directly onto the gameObject transform.
            // Cinemachine automatically accounts for this base offset during its LateUpdate render step.
            transform.localPosition = _combinedPositionOffset;
            transform.localEulerAngles = _combinedRotationOffset;
        }

        protected virtual void ProcessShake(ShakeInstance instance)
        {
            // Calculate progress value from 0.0 to 1.0 down the lifespan of the shake
            float normalizedTime = instance.Timer.Ratio;

            // Extract the magnitude and frequency matching this specific step in the shake runtime
            float currentMagnitude = instance.Settings.Magnitude(normalizedTime);
            float currentFreq = instance.Settings.Freq(normalizedTime);

            // Compute a continuous pseudo-random wave calculation utilizing Perlin Noise.
            // Using continuous unique offsets ensures smooth wave motion instead of jarring spatial jumps.
            float seed = instance.TimeOfStart;

            float xOffset = (Mathf.PerlinNoise(seed + Time.time * currentFreq, 0f) * 2f - 1f) * currentMagnitude;
            float yOffset = (Mathf.PerlinNoise(0f, seed + Time.time * currentFreq) * 2f - 1f) * currentMagnitude;
            float zRotOffset = (Mathf.PerlinNoise(seed, seed + Time.time * currentFreq) * 2f - 1f) * currentMagnitude *
                               0.5f;

            // Combine into class properties so multiple active shakes stack additively
            _combinedPositionOffset += new Vector3(xOffset, yOffset, 0f);
            _combinedRotationOffset += new Vector3(0f, 0f, zRotOffset);
        }

        public void Shake(ShakeSettings settings)
        {
            Instances.Add(new ShakeInstance()
            {
                Timer = new FiniteTimer(settings.Time),
                Settings = settings,
                TimeOfStart = UnityEngine.Time.time
            });
        }

        protected override void OnDispose()
        {
            cameraManager.RemovePet(this);
            base.OnDispose();
        }

        protected class ShakeInstance
        {
            public float TimeOfStart;
            public FiniteTimer Timer;
            public ShakeSettings Settings;
        }
    }
}
#endif