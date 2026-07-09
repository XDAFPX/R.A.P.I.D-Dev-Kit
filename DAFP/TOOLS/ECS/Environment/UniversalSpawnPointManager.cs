using System;
using System.Collections.Generic;
using System.Linq;
using Bdeshi.Helpers.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using ModestTree;
using NRandom;
using NRandom.Linq;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment
{
    public class UniversalSpawnPointManager : ISpawnPointManager
    {
        private SpawnPointPolicy policy;
        private IList<ISpawnPoint> pets = new List<ISpawnPoint>();
        private int index;
        [Inject] private IRandom rng;

        public UniversalSpawnPointManager(SpawnPointPolicy policy = SpawnPointPolicy.Shuffle)
        {
            this.policy = policy;
        }

        public IEnumerable<ISpawnPoint> Pets => pets;
        private int shuffled_idx = -1;

        public void Resolve()
        {
            if(pets.IsEmpty())
                return;
            switch (policy)
            {
                case SpawnPointPolicy.All:
                    pets.ForEach((point => point.Create().Forget()));
                    break;
                case SpawnPointPolicy.First:
                    pets.FirstOrDefault()?.Create().Forget();
                    break;
                case SpawnPointPolicy.Random:
                    pets.RandomElement(rng).Create();
                    break;
                case SpawnPointPolicy.Shuffle:
                    pets.ShuffledElement(ref shuffled_idx,rng)?.Create().Forget();
                    break;
                case SpawnPointPolicy.Multiplayer:
                    //TODO figure out
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void AddPet(ISpawnPoint pet)
        {
            GameUtils.AddPet(pet, pets);
        }

        public bool RemovePet(ISpawnPoint pet)
        {
            return GameUtils.RemovePet(pet, pets);
        }

        public enum SpawnPointPolicy
        {
            All,
            First,
            Random,
            Shuffle,
            Multiplayer
        }
    }
}