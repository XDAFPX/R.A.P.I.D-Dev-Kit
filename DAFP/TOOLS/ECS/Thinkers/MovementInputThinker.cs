using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Thinkers.IntegratedInput;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.ECS.Thinkers
{
    [CreateAssetMenu(menuName = "R.A.P.I.D/BuiltIn/Thinker/" + nameof(MovementInputThinker),
        fileName = nameof(MovementInputThinker))]
    public class MovementInputThinker : BaseThinker
    {
        IInputController controller;


        protected override void InternalInitialize(IEntity host)
        {
            controller = this.TryGetRootController((() => new InputController("Movement", InputSystem.actions)));
            controller.Bind("Move", (context) => OnMovementPerformed(host, context));
            controller.Enable();
        }


        protected override void InternalTick(IEntity host, ITickerBase ticker)
        {
        }

        private void OnMovementPerformed(IEntity host, InputAction.CallbackContext context)
        {
            if (host is ICommonEntityInterface.IEntMovementInputable _movement)
            {
                _movement.InputMovement((V2)context.ReadValue<Vector2>());
            }
        }

        protected override void InternalDispose(IEntity host)
        {
            controller?.Dispose();
        }

        protected override IEnumerable<IDebugDrawer> SetupDebugDrawers(IEntity host)
        {
            return default;
        }
    }
}