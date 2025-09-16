using System.Collections.Generic;
using System.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class SaveMetaSerializer : IMetaSerializer
    {
        private World world;

        public SaveMetaSerializer(World world)
        {
            this.world = world;
        }

        public async Task LoadMetaData(Dictionary<string, object> save)
        {
            save.ApplyConcreteDeserialization();
            if (save.TryGetValue("CurrentScene", out var _scene))
            {
                world.Initialize();
                await SceneManager.LoadSceneAsync((int)_scene);
            }
        }

        public Dictionary<string, object> SaveMetaData()
        {
            var _index = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            return new Dictionary<string, object>()
            {
                { "CurrentScene", _index }
            };
        }
    }
}