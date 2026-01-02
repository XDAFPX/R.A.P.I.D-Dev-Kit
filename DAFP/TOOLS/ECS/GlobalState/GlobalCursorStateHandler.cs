using System;
using System.Collections.Generic;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public abstract class GlobalCursorStateHandler : GlobalStateHandler<IGlobalCursorState>, IGlobalCursorStateHandler
    {
        public GlobalCursorStateHandler(string defaultState, IEventBus sus) : base(defaultState, sus)
        {
        }
    }

    public class BasicCursorState : StateBase, IGlobalCursorState
    {
        public BasicCursorState(IGlobalCursorStateHandler owner, CursorAnimation2D animation, string stateName)
        {
            Owner = owner;
            Animation = animation;
            StateName = stateName;
        }

        public override string StateName { get; }
        public override BtStatus LastStatus { get; } = BtStatus.Success;

        public override void EnterState()
        {
        }

        public override void Tick()
        {
            if (Animation != null)
                Animation.AnimateCursor();
            else
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public override void ExitState()
        {
        }

        public IGlobalCursorStateHandler Owner { get; }
        public CursorAnimation2D Animation { get; }
    }

    public class CursorStateChangeRequest : StateChangeRequest<IGlobalCursorState>
    {
        private readonly CursorLockMode mode;
        private readonly bool visible;

        public CursorStateChangeRequest(IGlobalCursorState state, int priority, string author, CursorLockMode mode,
            bool visible) :
            base(state, priority, author)
        {
            this.mode = mode;
            this.visible = visible;
        }

        public override void Apply(StateMachine<IGlobalCursorState> sm)
        {
            base.Apply(sm);
            Cursor.lockState = mode;
            Cursor.visible = visible;
        }
    }

    [CreateAssetMenu(fileName = "CursorAnim", menuName = "R.A.P.I.D/Cursor")]
    public class CursorAnimation2D : ScriptableObject //Stolen from GameDevBox (github)
    {
        public string name; // Name for clarity (optional)
        public Texture2D[] cursorFrames; // Animation frames for the cursor
        public float frameRate = 0.1f; // Animation speed
        public string[] objectTags; // Specify object tags to customize further (optional)

        protected float timer;
        protected int currentFrame;

        public void AnimateCursor()
        {
            if (cursorFrames.Length == 0)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer -= frameRate;
                currentFrame = (currentFrame + 1) % cursorFrames.Length;
                Cursor.SetCursor(cursorFrames[currentFrame], Vector2.zero, CursorMode.Auto);
            }
        }
    }

    public interface IGlobalCursorState : IState
    {
        public IGlobalCursorStateHandler Owner { get; }
        public CursorAnimation2D Animation { get; }
    }

    public interface IGlobalCursorStateHandler : Zenject.ITickable, Zenject.IInitializable,
        IGlobalStateHandler<IGlobalCursorState>
    {
    }
}