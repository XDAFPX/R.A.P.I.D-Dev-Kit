using System;
using DAFP.TOOLS.ECS.BuiltIn;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    public abstract class AbstractUniversalInputController : EntityComponent, IInputController
    {
        [SerializeField] private bool isLocked;

        protected override void OnInitialize()
        {
            IInputController.Controllers.Add(this);
        }


        public bool IsLocked => isLocked;

        public void Lock()
        {
            isLocked = true;
        }

        public void UnLock()
        {
            isLocked = false;
        }

        [field: GetComponentCache] public PlayerInput Input { get; }

        private IGamePlayer GPlayer;

        public void OnRegisterController(IGamePlayer player)
        {
            GPlayer = player;
            OnRegisterControllerInternal(player);
        }

        public abstract void OnRegisterControllerInternal(IGamePlayer pl);
    }
}