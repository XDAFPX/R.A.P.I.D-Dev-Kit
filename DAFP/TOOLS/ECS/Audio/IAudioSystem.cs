namespace DAFP.TOOLS.ECS.Audio
{
    public interface IAudioSystem
    {
        public IAudioInstance Play(IAudioSettings settings, string audio, params object[] additionaldata);
        public void DeleteInstance(IAudioInstance instance);
        public void PlayOneShot(IAudioSettings settings, string audio, params object[] additionaldata);
    }
}