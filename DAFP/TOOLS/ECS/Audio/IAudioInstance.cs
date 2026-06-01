using System;

namespace DAFP.TOOLS.ECS.Audio
{
    public interface IAudioInstance : IDisposable
    {
        public void Play();
        public void Stop();
        public float Volume { get; set; } // for runtime control
        public bool IsPlaying { get; }
    }
}