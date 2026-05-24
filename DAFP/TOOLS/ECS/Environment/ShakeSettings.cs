using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment
{
    [Serializable]
    public struct ShakeSettings
    {
        private readonly AnimationCurve magnitudeAC;
        private readonly AnimationCurve freqAC;
        private readonly float magnitude;
        private readonly float freq;
        public float Time;

        public float Magnitude(float t)
        {
            return magnitudeAC?.Evaluate(t) ?? magnitude;
        }

        public float Freq(float t)
        {
            return freqAC?.Evaluate(t) ?? freq;
        }

        public ShakeSettings(AnimationCurve magnitude, AnimationCurve freq, float time)
        {
            this.Time = time;
            this.magnitudeAC = magnitude;
            this.freqAC = freq;
            this.magnitude = 0;
            this.freq = 0;
        }

        public ShakeSettings(float magnitude, float freq, float time)
        {
            this.magnitude = magnitude;
            this.freq = freq;
            this.Time = time;


            this.magnitudeAC = null;
            this.freqAC = null;
        }
    }
}