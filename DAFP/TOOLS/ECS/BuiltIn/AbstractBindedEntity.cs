using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(PlayerInput))]
    public abstract class AbstractBindedEntity : StateDrivenEntity, IBindedEntity
    {
        // public abstract PlayerID PlayerID { get; } Convert the   InputAction.CallbackContext into a mirror class "InputBind" 
        public Dictionary<string, Action<InputBind.CallbackContext>> Binds { get; } = new();
        [field: GetComponentCache] public PlayerInput Input { get; }
        [field: GetComponentCache] public AbstractUniversalInputController Controller { get; }
        public Dictionary<InputBind, Action<InputBind.CallbackContext>> BindedActions { get; } = new();


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
            ((IBindedEntity)this).UnBindAll();
            base.OnDestroy();
        }

        protected abstract void SetInitialDataAfterBinds();
    }
}