using Archon.SwissArmyLib.Utils.Editor;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.BigData.Modifiers.Float;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    // modded script. Original by XeinTDM. Check him out he is cool asf.
    [RequireComponent(typeof(MouseSensitivityBoard))]
    [RequireComponent(typeof(FOVBoard))]
    [RequireComponent(typeof(FsmRunner))]
    public class FirstPersonCamera : StateDrivenEntity
    {
        [GetComponentCache] protected FOVBoard Fov;

        [GetComponentCache] protected MouseSensitivityBoard mouseSensitivity;

        [Tooltip("Reference to the player's body transform for rotation.")]
        public Transform playerBody;

        [Tooltip("The camera component attached to this object.")]
        public Camera playerCamera;

        // Internal variable to track the camera's x-axis rotation.
        private float xRotation = 0f;

        [Header("Head Bobbing Settings")] [Tooltip("Speed of head bobbing.")]
        public float bobbingSpeed = 0.18f;

        [Tooltip("Amount of head bobbing.")] public float bobbingAmount = 0.2f;

        [Tooltip("Midpoint of head bobbing on the y-axis.")]
        public float midpoint = 2f;


        [Tooltip("Field of view multiplier when sprinting.")] [ReadOnly(OnlyWhilePlaying = true)]
        public float sprintFOVMultiplier = 70f;

        protected MultiplyFloatModifier sprintMod;

        [Tooltip("Speed of FOV change.")] public float fovChangeSpeed = 5f;


        [Header("Camera Sway Settings")] [Tooltip("Amount of camera sway.")]
        public float swayAmount = 0.05f;

        [Tooltip("Speed of camera sway.")] public float swaySpeed = 4f;

        protected IState Normal => GetOrCreateState((Enter), null, (TickNormal), "NormalMode");
        protected IState Sprint => GetOrCreateState(((EnterSprint)), (ExitSprint ), ((TickSprint)), "SprintMode");

        private void ExitSprint()
        {
            Fov.RemoveModifier(sprintMod);
        }

        private void EnterSprint()
        {
            Fov.AddModifier(sprintMod);
        }

        private void TickSprint()
        {
            TickNormal();
        }

        protected virtual void Enter()
        {
        }

        private void TickNormal()
        {
            HandleMouseLook();
            HandleHeadBobbing();
            HandleFieldOfViewNormal();
            HandleCameraSway();
        }

        // Timer for head bobbing calculations.
        private float timer = 0.0f;

        protected Vector2 LastInput;
        protected Vector2 LastMovementInput;


        /// <summary>
        /// Handles the mouse look functionality.
        /// </summary>
        void HandleMouseLook()
        {
            float mouseX = LastInput.x * mouseSensitivity.Value * EntityTicker.DeltaTime;
            float mouseY = LastInput.y * mouseSensitivity.Value * EntityTicker.DeltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        /// <summary>
        /// Handles the head bobbing effect.
        /// </summary>
        void HandleHeadBobbing()
        {
            float waveslice = 0.0f;
            float horizontal = LastMovementInput.x;
            float vertical = LastMovementInput.y;

            if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
            {
                timer = 0.0f;
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer = timer + bobbingSpeed;
                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                }
            }

            Vector3 localPos = transform.localPosition;
            if (waveslice != 0)
            {
                float translateChange = waveslice * bobbingAmount;
                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                localPos.y = midpoint + translateChange;
            }
            else
            {
                localPos.y = midpoint;
            }

            transform.localPosition = localPos;
        }

        /// <summary>
        /// Handles the dynamic field of view.
        /// </summary>
        void HandleFieldOfViewNormal()
        {
            playerCamera.fieldOfView =
                Mathf.Lerp(playerCamera.fieldOfView, Fov.Value, fovChangeSpeed * Time.deltaTime);
        }


        /// <summary>
        /// Handles the camera sway effect.
        /// </summary>
        void HandleCameraSway()
        {
            float movementX = Mathf.Clamp(LastInput.x * swayAmount, -swayAmount, swayAmount);
            float movementY = Mathf.Clamp(LastInput.y * swayAmount, -swayAmount, swayAmount);
            Vector3 finalPosition = new Vector3(movementX, movementY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + finalPosition,
                swaySpeed * Time.deltaTime);
        }

        [Inject(Id = "DefaultUpdateEntityGameplayTicker")]public override ITicker<IEntity> EntityTicker { get; }

        protected override void SetInitialData()
        {
            sprintMod = new MultiplyFloatModifier(new QuikStat<float>(sprintFOVMultiplier),this);
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

        protected override void AddInitialStates(ref StateMachine<IState> sm)
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

    }
}