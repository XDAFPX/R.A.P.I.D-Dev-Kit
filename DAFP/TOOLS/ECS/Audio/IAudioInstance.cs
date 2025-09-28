using System;

namespace DAFP.TOOLS.ECS.Audio
{
    public interface IAudioInstance : IDisposable
    {
        
        public void Play();
        public void Stop();
    }
}