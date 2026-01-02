using System;
using Archon.SwissArmyLib.Utils.Editor;
using BDeshi.BTSM;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Components.GrabController;
using PixelRouge.Inspector;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    // [RequireComponent(typeof(CarryWeightBoard))]
    public class UniversalGrabController : EntityComponent, IGrabController
    {
        [SerializeField] private GrabController Controller;
        [SerializeField] private float DetachDistance = 5;

        [RequireStat] [InjectStat("CarryWeight")]
        private IStat<float> board;

        public bool CheckForLoS;

        [SerializeField] [HideIf(nameof(CheckForLoS), false)]
        private LayerMask Mask;

        [Inject(Id = "DefaultPhysicsComponentGameplayTicker")]
        public override ITickerBase EntityComponentTicker { get; }

        protected override void OnTick()
        {
            Simulate();
        }

        protected override void OnInitialize()
        {
            Controller.OnAttached += OnAttached;
            Controller.OnDeattached += OnDeattached;
        }


        private void OnDestroy()
        {
            Controller.OnAttached -= OnAttached;
            Controller.OnDeattached -= OnDeattached;
            Controller.Detach();
        }

        protected override void OnStart()
        {
        }

        private IBtNode Los;

        private bool HasIssueWithLoS()
        {
            if (!CheckForLoS)
                return false;
            if (Los.Tick() == BtStatus.Failure)
                return true;
            return false;
        }

        public void Simulate()
        {
            if (Controller.AttachedRigidbody == null)
                return;
            if (Vector3.Distance(Controller.AttachedRigidbody.transform.position, transform.position) >
                DetachDistance || board.Value < Controller.AttachedRigidbody.mass || HasIssueWithLoS())
            {
                Detach();
                return;
            }

            Controller.Simulate();
        }

        public void Attach(GameObject obj)
        {
            if (obj.TryGetComponent(out Rigidbody body))
            {
                if (CheckForLoS)
                    if (body.TryGetComponent<IEntity>(out var _entity))
                        Los = new EntBehNodes.LoSCheckNode3D(new BlackBoard(_entity, Host), Mask);

                if (board.Value >= body.mass)
                    Controller.Attach(obj);
            }
        }

        public void Detach()
        {
            Controller.Detach();
        }

        public float DefaultHoldDistance
        {
            get => Controller.DefaultHoldDistance;
            set => Controller.DefaultHoldDistance = value;
        }

        public float MassWhileCarring
        {
            get => Controller.MassWhileCarring;
            set => Controller.MassWhileCarring = value;
        }

        public float Damping
        {
            get => Controller.Damping;
            set => Controller.Damping = value;
        }

        public float MaxExitVelocity
        {
            get => Controller.MaxExitVelocity;
            set => Controller.MaxExitVelocity = value;
        }

        public Rigidbody AttachedBody => Controller.AttachedBody;

        public Transform Cam
        {
            get => Controller.Cam;
            set => Controller.Cam = value;
        }

        public event Action<Rigidbody> OnAttached;
        public event Action<Rigidbody> OnDeattached;
    }
}