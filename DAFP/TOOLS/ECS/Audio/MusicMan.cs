using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Audio
{
    public class MusicMan : IMusicMan
    {
        public void Dispose()
        {
            Stop();
            music?.Dispose();
            ambience?.Dispose();
        }

        public void Play()
        {
            ambience?.Play();
            music?.Play();
        }

        public void Stop()
        {
            ambience?.Stop();
            music?.Stop();
        }


        public bool IsPlaying => music.IsPlaying || ambience.IsPlaying;
        public string Name { get; set; }
        private readonly IAudioSystem audio;
        private IAudioInstance music;
        private IAudioInstance ambience;

        public MusicMan([Inject] IAudioSystem audio, string name)
        {
            Name = name;
            this.audio = audio;
        }

        private float _volume = 1f;

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (music != null) music.Volume = _volume * musicVolume;
                if (ambience != null) ambience.Volume = _volume * ambienceVolume;
            }
        }

        private float musicVolume = 1f;
        private float ambienceVolume = 1f;

        public void PlayMusic(string track, float fadeIn = 1f)
        {
            var old = music;
            music = audio.Play(audio.GetDefault(), track);
            music.Volume = 0f;

            if (old != null)
                old.CrossFadeTo(music, fadeIn).ContinueWith(() => old.Dispose()).Forget();
            else
                music.FadeTo(_volume * musicVolume, fadeIn).Forget();
        }

        public void PlayAmbience(string track, float fadeIn = 1f)
        {
            var old = ambience;
            ambience = audio.Play(audio.GetDefault(), track);
            ambience.Volume = 0f;

            if (old != null)
                old.CrossFadeTo(ambience, fadeIn).ContinueWith(() => old.Dispose()).Forget();
            else
                ambience.FadeTo(_volume * ambienceVolume, fadeIn).Forget();
        }

        public void StopMusic(float fadeOut = 1f) =>
            music?.FadeTo(0f, fadeOut).ContinueWith(() => music?.Stop()).Forget();

        public void StopAmbience(float fadeOut = 1f) =>
            ambience?.FadeTo(0f, fadeOut).ContinueWith(() => ambience?.Stop()).Forget();

        public void Fade(float targetVolume, float duration) =>
            this.FadeManager(targetVolume, duration).Forget();

        private async UniTaskVoid FadeManager(float targetVolume, float duration)
        {
            float start = _volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Volume = Mathf.Lerp(start, targetVolume, elapsed / duration); // uses setter
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            Volume = targetVolume;
        }
    }
}