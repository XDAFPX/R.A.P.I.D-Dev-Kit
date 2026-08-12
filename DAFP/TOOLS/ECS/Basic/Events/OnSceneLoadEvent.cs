using DAFP.TOOLS.ECS.Services;
using UnityEngine.SceneManagement;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnSceneLoadEvent
    {
        public OnSceneLoadEvent(Scene scene)
        {
            Scene = scene;
        }

        public Scene Scene { get; }

        public override string ToString()
        {
            return $"A Scene was loaded with (Name: \"{Scene.name}\", BuildIndex: {Scene.buildIndex}, Path: \"{Scene.path}\", IsLoaded: {Scene.isLoaded})";
        }
    }
}
