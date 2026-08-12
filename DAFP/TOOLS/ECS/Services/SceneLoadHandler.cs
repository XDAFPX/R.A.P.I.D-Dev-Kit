using System;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.Basic.Events;
using MessagePipe;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    public class SceneLoadHandler : IInitializable, IDisposable
    {
        [Inject] private IPublisher<OnSceneLoadEvent> e;

        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Catch scenes that were loaded before we subscribed
            // (e.g. this handler initializes after sceneLoaded already fired).
            int count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    e.Publish(new(scene));
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            e.Publish(new(scene));
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
