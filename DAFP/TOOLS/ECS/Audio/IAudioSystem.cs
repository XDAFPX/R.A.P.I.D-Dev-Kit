using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Audio
{
    public interface IAudioSystem
    {
        public IAudioSettings GetDefault();
        public IAudioInstance Play(IAudioSettings settings, string audio);
        public void DeleteInstance(IAudioInstance instance);
        public void PlayOneShot(IAudioSettings settings, string audio);
    }


    public interface IMusicMan : IAudioInstance, INameable
    { 
        void PlayMusic(string track, float fadeIn = 1f);
        void PlayAmbience(string track, float fadeIn = 1f);
        void StopMusic(float fadeOut = 1f);
        void StopAmbience(float fadeOut = 1f);
    }
}