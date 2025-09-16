using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(PlayerInput))]
    public abstract class AbstractGamePlayer : StateDrivenEntity, IGamePlayer
    {
        // public abstract PlayerID PlayerID { get; } Convert the   InputAction.CallbackContext into a mirror class "InputBind" 
        public Dictionary<string, Action<InputAction.CallbackContext>> Binds { get; } = new();
        [field: GetComponentCache] public PlayerInput Input { get; }
        [field: GetComponentCache] public AbstractUniversalInputController Controller { get; }
        public Dictionary<InputAction, Action<InputAction.CallbackContext>> BindedActions { get; } = new();


        protected sealed override void SetInitialData()
        {
            BindNameInitializer.Initialize(this);
            if (Controller != null)
                Controller.OnRegisterController(this);
            else
            {
                throw new Exception("you forgot to attach your controller to your player!");
            }

            SetInitialDataAfterBinds();
        }

        protected override void OnDestroy()
        {
            ((IGamePlayer)this).UnBindAll();
            base.OnDestroy();
        }

        protected abstract void SetInitialDataAfterBinds();
    }
}