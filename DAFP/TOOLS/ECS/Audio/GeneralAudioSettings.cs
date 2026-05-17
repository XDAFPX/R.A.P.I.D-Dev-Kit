using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Audio
{
    public struct GeneralAudioSettings : IAudioSettings
    {
        private readonly PositionTarget target;

        public GeneralAudioSettings(PositionTarget target, float volume=1, float pitch=1)
        {
            this.target = target;
            Volume = volume;
            Pitch = pitch;
        }

        public float Volume { get; }
        public float Pitch { get; }
        public PositionTarget AttachedToPosition()
        {
            return target;
        }
    }
}