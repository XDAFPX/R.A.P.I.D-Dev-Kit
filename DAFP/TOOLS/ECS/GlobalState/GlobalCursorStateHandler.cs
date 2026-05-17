using System;
using System.Collections.Generic;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public abstract class CursorStateHandler : GlobalStateHandler<IGlobalCursorState>, ICursorStateHandler
    {
    }

    public class BasicCursorState : StateBase, IGlobalCursorState
    {
        public BasicCursorState(CursorAnimation2D animation, CursorSettings settings, string stateName)
        {
            this.settings = settings;
            Animation = animation;
            StateName = stateName;
        }

        public override string StateName { get; }
        public override BtStatus LastStatus { get; } = BtStatus.Success;
        
        private readonly CursorSettings settings;

        public override void EnterState()
        {
        }

        public override void Tick()
        {
            if (Animation != null)
                Animation.AnimateCursor();
            else
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.lockState = settings.Mode;
            Cursor.visible = settings.IsVisible;
        }

        public override void ExitState()
        {
        }

        public CursorAnimation2D Animation { get; }
    }

    public struct CursorSettings
    {
        public CursorSettings(CursorLockMode mode, bool isVisible)
        {
            Mode = mode;
            IsVisible = isVisible;
        }

        public CursorLockMode Mode { get; init; }
        public bool IsVisible { get; init; }
    }

    public interface IGlobalCursorState : IState
    {
        public CursorAnimation2D Animation { get; }
    }

    public interface ICursorStateHandler : Zenject.ITickable, Zenject.IInitializable,
        IGlobalStateHandler<IGlobalCursorState>
    {
    }
}