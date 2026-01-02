using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codice.CM.WorkspaceServer.DataStore.IncomingChanges;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Services;
using NRandom;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class SaveMetaSerializer : IMetaSerializer
    {
        private World world;
        private readonly IRandom rng;

        public SaveMetaSerializer(World world, IRandom RNG)
        {
            this.world = world;
            rng = RNG;
        }

        public async Task LoadMetaData(Dictionary<string, object> save)
        {
            save.ApplyConcreteDeserialization();
            if (save.TryGetValue("CurrentScene", out var _scene))
            {
                world.Initialize();
                await SceneManager.LoadSceneAsync((int)_scene);
            }

            if (save.TryGetValue("Seed", out var seed)) rng.InitState((uint)Convert.ToInt32(seed));
        }

        public void TryChangeCurrentScene(Dictionary<string, object> save, int scene)
        {
            save.ApplyConcreteDeserialization();
            save["CurrentScene"] = scene;
        }

        public Dictionary<string, object> SaveMetaData(Dictionary<string, object> save)
        {
            var _index = SceneManager.GetActiveScene().buildIndex;

            var seed = (int)RandomEx.Shared.NextUInt();
            if (save != null && save.TryGetValue("Seed", out var _value)) seed = Convert.ToInt32(_value);

            return new Dictionary<string, object>
            {
                { "CurrentScene", _index }, { "Seed", (uint)seed }
            };
        }
    }
}