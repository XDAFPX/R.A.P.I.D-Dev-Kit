using UnityEngine.InputSystem;

namespace DAFP.TOOLS.Common
{
    /// <summary>
    /// A lightweight replica of CallbackContext that is easy to new-up in code or tests.
    /// </summary>
    public struct InputBind
    {
        /// <summary>Action’s name (ctx.action.name)</summary>
        public string ActionName { get; }

        /// <summary>The current phase (Started, Performed, Canceled)</summary>
        public InputActionPhase Phase { get; }

        /// <summary>Optional raw value (float, Vector2, etc.)</summary>
        public object Value { get; }

        /// <summary>True if Phase == Started</summary>
        public bool Started => Phase == InputActionPhase.Started;

        /// <summary>True if Phase == Performed</summary>
        public bool Performed => Phase == InputActionPhase.Performed;

        /// <summary>True if Phase == Canceled</summary>
        public bool Canceled => Phase == InputActionPhase.Canceled;

        /// <summary>
        /// Direct constructor for tests or manual invocation.
        /// </summary>
        public InputBind(string actionName, InputActionPhase phase, object value = null)
        {
            ActionName = actionName;
            Phase = phase;
            Value = value;
        }

        /// <summary>
        /// Copies data from a real CallbackContext.
        /// </summary>
        public InputBind(InputAction.CallbackContext ctx)
        {
            ActionName = ctx.action?.name;
            Phase = ctx.phase;
            // attempt to read value as object
            object v;
            switch (ctx.control?.valueType.Name)
            {
                case "Single":
                    v = ctx.ReadValue<float>();
                    break;
                case "Vector2":
                    v = ctx.ReadValue<UnityEngine.Vector2>();
                    break;
                case "Vector3":
                    v = ctx.ReadValue<UnityEngine.Vector3>();
                    break;
                default:
                    v = null;
                    break;
            }
            Value = v;
        }

        /// <summary>
        /// Safely cast Value to the requested type or default(T).
        /// </summary>
        public T ReadValue<T>() => Value is T t ? t : default;
    }
}
