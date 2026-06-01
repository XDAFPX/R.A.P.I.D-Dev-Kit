using System.Collections.Generic;
using System.Linq;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.ViewModel;
using DAFP.TOOLS.Injection;
using ModestTree;
using NUnit.Framework;
using RapidLib.DAFP.TOOLS.Common;
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

        private Camera output => brain.OutputCamera ?? brain.GetComponent<Camera>();

        public void UpdateSubjects(IEnumerable<IEntity> subjects)
        {
            set_camera_targets(subjects);
        }

        public Rect Rect
        {
            get => output.rect;
            set => output.rect = value;
        } //Implement


        private ITargetOf<IEntity> _lockedTarget;

         private static readonly CameraSubjectPolicy policy = CameraSubjectPolicy.FollowAll;

        private void set_camera_targets(IEnumerable<IEntity> subjects)
        {
            var _subjects = subjects.ToList();
            if (_subjects.IsEmpty()) return;

            if (brain.ActiveVirtualCamera is not CinemachineCamera _cam) return;

            switch (policy)
            {
                case CameraSubjectPolicy.FollowAll:
                {
                    var _group = get_or_create_target_group();
                    _group.Targets.Clear();
                    foreach (var _subject in _subjects)
                    {
                        var _go = _subject.GetWorldRepresentation();
                        if (_go == null) continue;
                        _group.Targets.Add(new CinemachineTargetGroup.Target
                        {
                            Object = _go.transform,
                            Weight = 1f,
                            Radius = 1f
                        });
                    }

                    _cam.Follow = _group.transform;
                    _cam.LookAt = _group.transform;
                    break;
                }
                case CameraSubjectPolicy.FollowMain:
                {
                    var _target = _subjects.Players(GameUtils.PlayerSelectionPolicy.SingleOut)
                        .Select(p => p?.Body).FirstOrDefault();
                    set_single_target(_cam, _target);
                    break;
                }
                case CameraSubjectPolicy.FollowOnlyOne:
                {
                    if (!_lockedTarget.HasValue || _lockedTarget.Raw?.GetWorldRepresentation() == null)
                    {
                        var _first = _subjects.FirstOrDefault(s => s.GetWorldRepresentation() != null);
                        _lockedTarget = _first != null ? new TargetOf<IEntity>(_first) : null;
                    }

                    if (_lockedTarget != null)
                        set_single_target(_cam, _lockedTarget.Raw);
                    break;
                }
                case CameraSubjectPolicy.FollowClosest:
                {
                    var _target = _subjects
                        .Where(s => s.GetWorldRepresentation() != null)
                        .OrderBy(s =>
                            Vector3.Distance(transform.position, s.GetWorldRepresentation().transform.position))
                        .FirstOrDefault();
                    set_single_target(_cam, _target);
                    break;
                }
                case CameraSubjectPolicy.FollowAverageCentroid:
                {
                    var _valid = _subjects.Select(s => s.GetWorldRepresentation()).Where(g => g != null).ToList();
                    if (_valid.IsEmpty()) break;
                    var _centroid = _valid.Aggregate(Vector3.zero, (acc, g) => acc + g.transform.position) /
                                    _valid.Count;
                    var _dummy = get_or_create_dummy();
                    _dummy.position = _centroid;
                    _cam.Follow = _dummy;
                    _cam.LookAt = _dummy;
                    break;
                }
            }
        }

        private void set_single_target(CinemachineCamera cam, IEntity target)
        {
            var _go = target?.GetWorldRepresentation();
            if (_go == null) return;
            cam.Follow = _go.transform;
            cam.LookAt = _go.transform;
        }

        private Transform _dummy;

        private Transform get_or_create_dummy()
        {
            if (_dummy != null) return _dummy;
            _dummy = new GameObject("CameraCentroidDummy").transform;
            _dummy.SetParent(transform);
            return _dummy;
        }

        private CinemachineTargetGroup get_or_create_target_group()
        {
            var _group = GetComponentInChildren<CinemachineTargetGroup>();
            if (_group != null) return _group;
            var _go = new GameObject("CameraTargetGroup");
            _go.transform.SetParent(transform);
            return _go.AddComponent<CinemachineTargetGroup>();
        }

        protected enum CameraSubjectPolicy
        {
            FollowAll, // target group, weights all subjects
            FollowMain, // follows the main/single player
            FollowOnlyOne, // locks to first valid subject forever
            FollowClosest, // always follows whoever is closest to camera
            FollowAverageCentroid // follows the average position without target group
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