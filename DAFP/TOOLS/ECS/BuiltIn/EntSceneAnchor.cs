using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EntSceneAnchor : EmptyEntity
    {
        [SerializeField] private SceneReference scene;

        [Inject] private ZenjectSceneLoader _sceneLoader;

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            load_sub_scene_async(scene.Name);
        }


        private void load_sub_scene_async(string sceneName)
        {
            if(Application.isEditor)
                return;
            if (string.IsNullOrEmpty(sceneName) || SceneManager.GetSceneByName(sceneName).isLoaded)
                return;

            _sceneLoader.LoadScene(sceneName, LoadSceneMode.Additive);
            
        }
    }
}