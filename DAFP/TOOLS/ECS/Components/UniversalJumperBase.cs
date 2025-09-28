using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData.Common;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    /// <summary>
    /// Shared jump-buffer and coyote-time logic for any vector type.
    /// </summary>
    public abstract class UniversalJumperBase<TVec> : EntityComponent
    {
        [GetComponentCache] private JumpStrengthBoard jumpStrengthBoard;
        [GetComponentCache] private UniversalCooldownController cooldownController;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private Cooldown CoyoteTime;

        [ReadOnly(OnlyWhilePlaying = true)] [SerializeField]
        private Cooldown JumpBuffer;

        [Tooltip("Divisor applied when cutting the jump")] [SerializeField]
        private float CutJumpDivisor = 2f;

        protected override void OnInitialize()
        {
            cooldownController.RegisterCooldown(CoyoteTime);
            cooldownController.RegisterCooldown(JumpBuffer);
        }

        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
        }

        /// <summary>Call every frame while grounded.</summary>
        public void TickGround()
        {
            CoyoteTime.SetToMin();
            TryJump();
        }

        /// <summary>Call every frame while in air.</summary>
        public void TickAir()
        {
            TryJump();
        }

        private void TryJump()
        {
            if (!JumpBuffer.isComplete && !CoyoteTime.isComplete)
            {
                JumpBuffer.SetToMax();
                PerformJump(MultiplyVector(UpVector, jumpStrengthBoard.Value));
            }
        }

        /// <summary>Bind to your input system’s OnJump action.</summary>
        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                // buffer jump press
                JumpBuffer.SetToMin();
                CoyoteTime.SetToMax();
            }
            else if (ctx.canceled)
            {
                // cut jump early
                PerformCutJump(UpVector, CutJumpDivisor);
            }
        }


        /// <summary>Unit up vector for this mover (e.g. Vector2 or Vector3).</summary>
        protected abstract TVec UpVector { get; }

        protected abstract TVec MultiplyVector(TVec vec, float m);

        /// <summary>Run the actual jump impulse/force.</summary>
        protected abstract void PerformJump(TVec force);

        /// <summary>Run the cut-jump (dividing velocity).</summary>
        protected abstract void PerformCutJump(TVec up, float divisor);
    }
}