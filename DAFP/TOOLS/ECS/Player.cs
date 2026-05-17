using System.Collections.Generic;
using DAFP.TOOLS.BTs;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;

namespace DAFP.TOOLS.ECS
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity))]
    public sealed class Player : EntityComponent, IPlayer
    {
        public string Name
        {
            get => Body.Name;
            set => Body.Name = value;
        }
        protected override void OnInitialize()
        {
            if(Data.Memory !=null)
                return;
            Data = Data.SetData(new BlackBoard(Host));
        }

        [field: SerializeField] public PlayerData Data { get; internal set; }
        public IEntity Body => Host;


        public ISaveData Save()
        {
            return Data.Memory.Save();
        }

        public void Load(ISaveData saveData)
        {
            Data.Memory.Load(saveData);
        }


        protected override void OnTick()
        {
        }
    }
}