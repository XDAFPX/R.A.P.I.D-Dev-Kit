using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
public class InfoSystem : ITickable
{
    public float CurrentFPS { get; private set; }

    public void Tick()
    {
        if (Time.unscaledDeltaTime > 0)
        {
            CurrentFPS = 1.0f / Time.unscaledDeltaTime;
        }
    }
}
}