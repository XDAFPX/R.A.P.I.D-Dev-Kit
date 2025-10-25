namespace DAFP.TOOLS.ECS.Audio
{
    public interface IAudioSystem
    {
        public IAudioSettings GetDefault();
        public IAudioInstance Play(IAudioSettings settings, string audio);
        public void DeleteInstance(IAudioInstance instance);
        public void PlayOneShot(IAudioSettings settings, string audio);
    }
}