using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using FluentResults;
using NRandom;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class SaveMetaSerializer : ISerializer<GameMetaData>
    {
        private World world;
        private readonly IRandom rng;

        public SaveMetaSerializer(World world, IRandom RNG)
        {
            this.world = world;
            rng = RNG;
        }

        // public async Task LoadMetaData(Dictionary<string, object> save)
        // {
        //     save.ApplyConcreteDeserialization();
        //     if (save.TryGetValue("CurrentScene", out var _scene))
        //     {
        //         world.Initialize();
        //         await SceneManager.LoadSceneAsync((int)_scene);
        //     }
        //
        //     if (save.TryGetValue("Seed", out var seed)) rng.InitState((uint)Convert.ToInt32(seed));
        // }
        //
        // public void TryChangeCurrentScene(Dictionary<string, object> save, int scene)
        // {
        //     save.ApplyConcreteDeserialization();
        //     save["CurrentScene"] = scene;
        // }
        //
        // public Dictionary<string, object> SaveMetaData(Dictionary<string, object> save)
        // {
        //     var _index = SceneManager.GetActiveScene().buildIndex;
        //
        //     var seed = (int)RandomEx.Shared.NextUInt();
        //     if (save != null && save.TryGetValue("Seed", out var _value)) seed = Convert.ToInt32(_value);
        //
        //     return new Dictionary<string, object>
        //     {
        //         { "CurrentScene", _index }, { "Seed", (uint)seed }
        //     };
        // }

        public Result<ISaveData> Serialize(GameMetaData obj)
        {
            throw new NotImplementedException();
        }

        public Result DeSerialize(ISaveData save, GameMetaData ent)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GameMetaData : ISavable
    {
        private readonly Dictionary<string, object> _data;

        public GameMetaData(Dictionary<string, object> initial = null)
            => _data = initial ?? new Dictionary<string, object>();


        public int Seed
        {
            get => _data.TryGetValue("Seed", out var v) ? (int)v : 0;
            set => _data["Seed"] = value;
        }

        public string Scene
        {
            get => _data.TryGetValue("Scene", out var v) ? (string)v : null;
            set => _data["Scene"] = value;
        }

        // strongly-typed accessors for known fields
        public long Ticks
        {
            get => _data.TryGetValue("Ticks", out var v) ? (long)v : 0;
            set => _data["Ticks"] = value;
        }

        public float GameHours
        {
            get => _data.TryGetValue("GameHours", out var v) ? (float)v : 0f;
            set => _data["GameHours"] = value;
        }

        public ISaveData Save() => new GenericSaveData(new Dictionary<string, object>(_data));

        public void Load(ISaveData saveData)
        {
            foreach (var key in saveData.Keys)
            {
                saveData.TryGet(key, out var value);
                _data[key] = value;
            }
        }

        // "future variants" become factory methods, not subclasses
        public static GameMetaData NewGame(string scene) => new GameMetaData { Ticks = 0, GameHours = 0f, Seed = RandomEx.Shared.NextInt(), Scene = scene};
    }
}