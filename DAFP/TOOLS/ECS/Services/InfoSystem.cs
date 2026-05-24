using System;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    public class InfoSystem : ITickable
    {
        public float CurrentFPS { get; private set; }
        public float TimeFromStart { get; private set; }
        public TimeSpan TimeSpanFromStart => System.TimeSpan.FromSeconds(TimeFromStart);
        public DateTime GameStart { get; private set; }

        public void Tick()
        {
            if (TimeFromStart == 0)
            {
                GameStart= DateTime.Now;
            }
            if (Time.unscaledDeltaTime > 0)
            {
                TimeFromStart += Time.unscaledDeltaTime;
                CurrentFPS = 1.0f / Time.unscaledDeltaTime;
            }
        }
    }
}