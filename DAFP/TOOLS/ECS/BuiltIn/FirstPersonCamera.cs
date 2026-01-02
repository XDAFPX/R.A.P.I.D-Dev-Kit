using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using BDeshi.BTSM;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers.Float;
using DAFP.TOOLS.ECS.Services;
using DAFP.TOOLS.ECS.ViewModel;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    // modded script. Original by XeinTDM. Check him out he is cool asf.
    [RequireComponent(typeof(FsmRunner))]
    public class FirstPersonCamera : StateDrivenEntity
    {
        [InjectStat("FOV", 90)] protected IStat<float> Fov;

        [RequireStat][InjectStat("MouseSensitivity")] protected IStat<float> MouseSensitivity;

        [Tooltip("Reference to the player's body transform for rotation.")]
        public Transform PlayerBody;

        [Tooltip("The camera component attached to this object.")]
        public Camera PlayerCamera;

        // Internal variable to track the camera's x-axis rotation.
        private float xRotation = 0f;

        [Header("Head Bobbing Settings")] [Tooltip("Speed of head bobbing.")]
        public float BobbingSpeed = 0.18f;

        [Tooltip("Amount of head bobbing.")] public float BobbingAmount = 0.2f;

        [Tooltip("Midpoint of head bobbing on the y-axis.")]
        public float Midpoint = 2f;


        [Tooltip("Field of view multiplier when sprinting.")] [ReadOnly(OnlyWhilePlaying = true)]
        public float SprintFOVMultiplier = 70f;

        protected MultiplyFloatModifier SprintMod;

        [Tooltip("Speed of FOV change.")] public float FOVChangeSpeed = 5f;


        [Header("Camera Sway Settings")] [Tooltip("Amount of camera sway.")]
        public float SwayAmount = 0.05f;

        [Tooltip("Speed of camera sway.")] public float SwaySpeed = 4f;

        protected IState Normal => GetOrCreateState(Enter, null, tick_normal, "NormalMode");
        protected IState Sprint => GetOrCreateState(enter_sprint, exit_sprint, tick_sprint, "SprintMode");

        private void exit_sprint()
        {
            Fov.RemoveModifier(SprintMod);
        }

        private void enter_sprint()
        {
            Fov.AddModifier(SprintMod);
        }

        private void tick_sprint()
        {
            tick_normal();
        }

        protected virtual void Enter()
        {
        }

        private void tick_normal()
        {
            handle_mouse_look();
            handle_head_bobbing();
            handle_field_of_view_normal();
            handle_camera_sway();
        }

        // Timer for head bobbing calculations.
        private float timer = 0.0f;

        protected Vector2 LastInput;
        protected Vector2 LastMovementInput;

        /// <summary>
        /// Handles the mouse look functionality.
        /// </summary>
        private void handle_mouse_look()
        {
            var _mouseX = LastInput.x * MouseSensitivity.Value * EntityTicker.DeltaTime;
            var _mouseY = LastInput.y * MouseSensitivity.Value * EntityTicker.DeltaTime;

            xRotation -= _mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            PlayerBody.Rotate(Vector3.up * _mouseX);
        }

        /// <summary>
        /// Handles the head bobbing effect.
        /// </summary>
        private void handle_head_bobbing()
        {
            var _waveslice = 0.0f;
            var _horizontal = LastMovementInput.x;
            var _vertical = LastMovementInput.y;

            if (Mathf.Abs(_horizontal) == 0 && Mathf.Abs(_vertical) == 0)
            {
                timer = 0.0f;
            }
            else
            {
                _waveslice = Mathf.Sin(timer);
                timer = timer + BobbingSpeed;
                if (timer > Mathf.PI * 2) timer = timer - Mathf.PI * 2;
            }

            var _localPos = transform.localPosition;
            if (_waveslice != 0)
            {
                var _translateChange = _waveslice * BobbingAmount;
                var _totalAxes = Mathf.Abs(_horizontal) + Mathf.Abs(_vertical);
                _totalAxes = Mathf.Clamp(_totalAxes, 0.0f, 1.0f);
                _translateChange = _totalAxes * _translateChange;
                _localPos.y = Midpoint + _translateChange;
            }
            else
            {
                _localPos.y = Midpoint;
            }

            transform.localPosition = _localPos;
        }

        /// <summary>
        /// Handles the dynamic field of view.
        /// </summary>
        private void handle_field_of_view_normal()
        {
            PlayerCamera.fieldOfView =
                Mathf.Lerp(PlayerCamera.fieldOfView, Fov.Value, FOVChangeSpeed * Time.deltaTime);
        }


        /// <summary>
        /// Handles the camera sway effect.
        /// </summary>
        private void handle_camera_sway()
        {
            var _movementX = Mathf.Clamp(LastInput.x * SwayAmount, -SwayAmount, SwayAmount);
            var _movementY = Mathf.Clamp(LastInput.y * SwayAmount, -SwayAmount, SwayAmount);
            var _finalPosition = new Vector3(_movementX, _movementY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + _finalPosition,
                SwaySpeed * Time.deltaTime);
        }

        public override NonEmptyList<IViewModel> SetupView()
        {
            return new NonEmptyList<IViewModel> { new EmptyView() };
        }

        [Inject(Id = "DefaultUpdateEntityGameplayTicker")]
        public override ITicker<IEntity> EntityTicker { get; }

        protected override void SetInitialData()
        {
            SprintMod = new MultiplyFloatModifier(new QuikStat<float>(SprintFOVMultiplier), this);
        }

        public void TransitionToNormalMode()
        {
            StateMachine.TransitionToInitialState();
        }

        public void TransitionToSprintMode()
        {
            StateMachine.ForceTakeTransition(transitionToSprint);
        }

        private ITransition<IState> transitionToSprint;

        protected override void RegisterInitialStates(ref StateMachine<IState> sm)
        {
            transitionToSprint = sm.AddManualTransitionTo(Sprint);
        }

        protected override IState GetInitialState()
        {
            return Normal;
        }

        protected override void StateDrivenEntityInitialize()
        {
        }

        public void OnMove(Vector2 vector2)
        {
            LastMovementInput = vector2;
        }

        public void OnLook(Vector2 vector2)
        {
            LastInput = vector2;
        }

        public override void Load(Dictionary<string, object> save)
        {
            base.Load(save);
            xRotation = transform.localRotation.eulerAngles.x;
        }
    }
}