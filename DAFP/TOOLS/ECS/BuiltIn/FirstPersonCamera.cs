using BDeshi.BTSM;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    // Refactored to take input via public methods, suitable for Unity's new Input System
    [RequireComponent(typeof(FsmRunner))]
    public class FirstPersonCamera : StateDrivenEntity
    {
        [Header("Mouse Look Settings")]
        [Tooltip("Sensitivity of mouse movement.")]
        public float mouseSensitivity = 100f;

        [Tooltip("Reference to the player's body transform for rotation.")]
        public Transform playerBody;

        [Tooltip("The camera component attached to this object.")]
        public Camera playerCamera;

        // Internal x-axis rotation tracker
        private float xRotation = 0f;

        [Header("Head Bobbing Settings")]
        [Tooltip("Speed of head bobbing.")]
        public float bobbingSpeed = 0.18f;

        [Tooltip("Amount of head bobbing.")]
        public float bobbingAmount = 0.2f;

        [Tooltip("Midpoint of head bobbing on the y-axis.")]
        public float midpoint = 2f;

        [Header("Field of View Settings")]
        [Tooltip("Normal field of view.")]
        public float baseFOV = 60f;

        [Tooltip("Field of view when sprinting.")]
        public float sprintFOV = 70f;

        [Tooltip("Speed of FOV change.")]
        public float fovChangeSpeed = 5f;

        [Tooltip("Flag to determine if the player is sprinting.")]
        public bool isSprinting = false;

        [Header("Camera Sway Settings")]
        [Tooltip("Amount of camera sway.")]
        public float swayAmount = 0.05f;

        [Tooltip("Speed of camera sway.")]
        public float swaySpeed = 4f;

        // Internal timers and input storage
        private float timer = 0f;
        private Vector2 lookInput = Vector2.zero;
        private Vector2 moveInput = Vector2.zero;

        // -- State Machine Definitions --

        private IState CameraUpdateState =>
            GetOrCreateState(null, null, CameraUpdateTick, "CameraUpdate");

        protected override IState GetInitialState() => CameraUpdateState;

        protected override void AddInitialStates(ref StateMachine<IState> sm)
        {
            // No transitions; single-state loop
        }

        protected override void SetInitialData()
        {
            // No additional data
        }

        protected override void StateDrivenEntityInitialize()
        {
            // Initialization logic (formerly Start)
            if (playerCamera == null)
            {
                Debug.LogError("PlayerCamera is not assigned in the inspector.");
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                playerCamera.fieldOfView = baseFOV;
            }
        }

        // Called every tick by the state machine
        private void CameraUpdateTick()
        {
            HandleMouseLook();      // uses lookInput
            HandleHeadBobbing();    // uses moveInput
            HandleFieldOfView();
            HandleCameraSway();     // uses lookInput
        }

        // -- Input System hooks --

        /// <summary>
        /// Call this from your InputAction callback for look, e.g. context.ReadValue&lt;Vector2&gt;()
        /// </summary>
        public void OnLook(Vector2 delta)
        {
            lookInput = delta;
        }

        /// <summary>
        /// Call this from your InputAction callback for move, e.g. WASD or gamepad stick
        /// </summary>
        public void OnMove(Vector2 movement)
        {
            moveInput = movement;
        }

        /// <summary>
        /// Call this when the sprint button is pressed/released
        /// </summary>
        public void OnSprint(bool sprinting)
        {
            isSprinting = sprinting;
        }

        // -- Handlers now using injected input --

        private void HandleMouseLook()
        {
            // lookInput assumed to be mouse delta; scale by sensitivity
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        private void HandleHeadBobbing()
        {
            float waveslice = 0f;
            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                timer = 0f;
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer += bobbingSpeed;
                if (timer > Mathf.PI * 2) timer -= Mathf.PI * 2;
            }

            Vector3 localPos = transform.localPosition;
            if (!Mathf.Approximately(waveslice, 0f))
            {
                float translateChange = waveslice * bobbingAmount;
                float totalAxes = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
                localPos.y = midpoint + totalAxes * translateChange;
            }
            else
            {
                localPos.y = midpoint;
            }

            transform.localPosition = localPos;
        }

        private void HandleFieldOfView()
        {
            if (playerCamera == null) return;

            float targetFOV = isSprinting ? sprintFOV : baseFOV;
            playerCamera.fieldOfView = 
                Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
        }

        private void HandleCameraSway()
        {
            // use lookInput for sway instead of raw Input.GetAxis
            float movementX = Mathf.Clamp(lookInput.x * swayAmount, -swayAmount, swayAmount);
            float movementY = Mathf.Clamp(lookInput.y * swayAmount, -swayAmount, swayAmount);
            Vector3 swayOffset = new Vector3(movementX, movementY, 0f);

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                transform.localPosition + swayOffset,
                swaySpeed * Time.deltaTime);
        }

        public override ITicker<IEntity> EntityTicker => World.UpdateTicker;
    }
}