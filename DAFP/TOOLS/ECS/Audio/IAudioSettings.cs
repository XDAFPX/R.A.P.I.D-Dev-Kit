using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Audio
{
    public  interface IAudioSettings
    {
        public float Volume { get; }
        public float Pitch { get; }
        public PositionTarget AttachedToPosition();
    }
}