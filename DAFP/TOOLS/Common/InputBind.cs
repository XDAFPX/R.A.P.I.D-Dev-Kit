using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.Common
{
    /// <summary>
    /// Wraps a Unity InputAction and exposes Started/Performed/Canceled
    /// events that fire with our own CallbackContext, whether from actual input
    /// or manually triggered. Implements IDisposable to unsubscribe cleanly.
    /// </summary>
    public class InputBind : IDisposable
    {
        private readonly InputAction _action;

        // Backing delegates for subscribers using our CallbackContext
        private event Action<CallbackContext> _startedHandlers;
        private event Action<CallbackContext> _performedHandlers;
        private event Action<CallbackContext> _canceledHandlers;

        // Named delegates for Unity subscription (convert Unity→our context)
        private readonly Action<InputAction.CallbackContext> _onStarted;
        private readonly Action<InputAction.CallbackContext> _onPerformed;
        private readonly Action<InputAction.CallbackContext> _onCanceled;

        private bool _disposed;

        /// <summary>
        /// Fired when the action starts (real or manual).
        /// </summary>
        public event Action<CallbackContext> started
        {
            add => _startedHandlers += value;
            remove => _startedHandlers -= value;
        }

        /// <summary>
        /// Fired when the action performs (real or manual).
        /// </summary>
        public event Action<CallbackContext> performed
        {
            add => _performedHandlers += value;
            remove => _performedHandlers -= value;
        }

        /// <summary>
        /// Fired when the action cancels (real or manual).
        /// </summary>
        public event Action<CallbackContext> canceled
        {
            add => _canceledHandlers += value;
            remove => _canceledHandlers -= value;
        }

        /// <summary>
        /// Wrap an existing InputAction and hook into its Unity callbacks.
        /// </summary>
        public InputBind(InputAction action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));

            // Convert Unity CallbackContext → our CallbackContext on each phase
            _onStarted = unityCtx => _startedHandlers?.Invoke(new CallbackContext(unityCtx));
            _onPerformed = unityCtx => _performedHandlers?.Invoke(new CallbackContext(unityCtx));
            _onCanceled = unityCtx => _canceledHandlers?.Invoke(new CallbackContext(unityCtx));

            _action.started += _onStarted;
            _action.performed += _onPerformed;
            _action.canceled += _onCanceled;
        }

        /// <summary>
        /// Create a new underlying InputAction.
        /// </summary>
        public InputBind(string name,
            InputActionType type = InputActionType.Button,
            string binding = null,
            string interactions = null,
            string processors = null,
            string expectedControlType = null)
            : this(new InputAction(name, type, binding, interactions, processors, expectedControlType))
        {
        }

        /// <summary>
        /// Enable the underlying action.
        /// </summary>
        public void Enable()
        {
            _action.Enable();
        }

        /// <summary>
        /// Disable the underlying action.
        /// </summary>
        public void Disable()
        {
            _action.Disable();
        }

        /// <summary>
        /// Manually invoke Started with a custom CallbackContext.
        /// </summary>
        public void TriggerStarted(CallbackContext context)
        {
            _startedHandlers?.Invoke(context);
        }

        /// <summary>
        /// Manually invoke Performed with a custom CallbackContext.
        /// </summary>
        public void TriggerPerformed(CallbackContext context)
        {
            _performedHandlers?.Invoke(context);
        }

        /// <summary>
        /// Manually invoke Canceled with a custom CallbackContext.
        /// </summary>
        public void TriggerCanceled(CallbackContext context)
        {
            _canceledHandlers?.Invoke(context);
        }

        /// <summary>
        /// Dispose unsubscribes all Unity callbacks and clears handlers.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _action.started -= _onStarted;
            _action.performed -= _onPerformed;
            _action.canceled -= _onCanceled;

            _startedHandlers = null;
            _performedHandlers = null;
            _canceledHandlers = null;

            _disposed = true;
        }

        public static InputBind From(InputAction action)
        {
            return new InputBind(action);
        }

        public static implicit operator InputAction(InputBind bind)
        {
            return bind._action;
        }

        public static implicit operator InputBind(InputAction action)
        {
            return new InputBind(action);
        }

        // --------------------------------------------------------------------
        // Nested CallbackContext mirror with multi-object UserData
        // --------------------------------------------------------------------
        /// <summary>
        /// A mirror of Unity's InputAction.CallbackContext that allows
        /// attaching multiple UserData objects and converting back if needed.
        /// </summary>
        public struct CallbackContext
        {
            private readonly InputAction.CallbackContext _unityContext;
            public bool performed => Phase == InputActionPhase.Performed;
            public bool canceled => Phase == InputActionPhase.Canceled;
            public bool started => Phase == InputActionPhase.Started;

            public InputAction Action { get; }
            public InputControl Control { get; }
            public InputActionPhase Phase { get; }
            public double Time { get; }
            public double StartTime { get; }

            /// <summary>
            /// A list of arbitrary payloads for AI or testing.
            /// </summary>
            public IReadOnlyList<object> UserData { get; }

            /// <summary>
            /// Construct from a real Unity CallbackContext with optional UserData.
            /// </summary>
            public CallbackContext(InputAction.CallbackContext unityContext, IEnumerable<object> userData = null)
            {
                _unityContext = unityContext;
                Action = unityContext.action;
                Control = unityContext.control;
                Phase = unityContext.phase;
                Time = unityContext.time;
                StartTime = unityContext.startTime;
                UserData = userData != null
                    ? new List<object>(userData)
                    : Array.Empty<object>();
            }

            /// <summary>
            /// Construct fully manually (no underlying Unity context).
            /// </summary>
            public CallbackContext(InputAction action,
                InputControl control,
                InputActionPhase phase,
                double time,
                double startTime,
                IEnumerable<object> userData = null)
            {
                _unityContext = default;
                Action = action;
                Control = control;
                Phase = phase;
                Time = time;
                StartTime = startTime;
                UserData = userData != null
                    ? new List<object>(userData)
                    : Array.Empty<object>();
            }

            /// <summary>
            /// Read as T: returns the first matching UserData entry if found,
            /// otherwise falls back to Unity's ReadValue&lt;T&gt;().
            /// </summary>
            public T ReadValue<T>() where T : struct
            {
                if (UserData != null)
                    foreach (var item in UserData)
                        if (item is T match)
                            return match;

                return _unityContext.ReadValue<T>();
            }

            /// <summary>
            /// Convert our mirror back to Unity's CallbackContext.
            /// </summary>
            public static implicit operator InputAction.CallbackContext(CallbackContext ctx)
            {
                return ctx._unityContext;
            }
        }
    }
}