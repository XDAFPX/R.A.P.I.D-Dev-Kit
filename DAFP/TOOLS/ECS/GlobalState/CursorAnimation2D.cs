using UnityEngine;

namespace DAFP.TOOLS.ECS.GlobalState
{
    [CreateAssetMenu(fileName = "CursorAnim", menuName = "R.A.P.I.D/Cursor")]
    public class CursorAnimation2D : ScriptableObject //Stolen from GameDevBox (github)
    {
        public string Name; // Name for clarity (optional)
        public Texture2D[] CursorFrames; // Animation frames for the cursor
        public float FrameRate = 0.1f; // Animation speed
        public string[] ObjectTags; // Specify object tags to customize further (optional)

        protected float Timer;
        protected int CurrentFrame;

        public void AnimateCursor()
        {
            if (CursorFrames.Length == 0)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }

            Timer += Time.deltaTime;
            if (Timer >= FrameRate)
            {
                Timer -= FrameRate;
                CurrentFrame = (CurrentFrame + 1) % CursorFrames.Length;
                Cursor.SetCursor(CursorFrames[CurrentFrame], Vector2.zero, CursorMode.Auto);
            }
        }
    }
}