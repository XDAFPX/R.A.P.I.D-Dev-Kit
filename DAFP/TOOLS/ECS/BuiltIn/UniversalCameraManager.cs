using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using Zenject;
#if CINEMAMACHINE
using Unity.Cinemachine;
#endif

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCameraManager : ScriptableObject, ICameraManager, IInitializable
    {
        [Inject(Id = "Cameras", Optional = true)]
        private IEnumerable<IGameCamera> cams;


        [SerializeField] private StatContainer stats;

        public IStatContainer Stats => stats;
        public IEnumerable<IEntity> Subjects => subjects;
        private HashSet<IEntity> subjects = new();


        public virtual void Initialize()
        {
        }

        public void AddSubject(IEntity ent)
        {
            if (subjects.Contains(ent)) return;
            subjects.Add(ent);
        }

        public void RemoveSubject(IEntity ent)
        {
            if (!subjects.Contains(ent)) return;
            subjects.Remove(ent);
        }


        public ISaveData Save()
        {
            throw new NotImplementedException(); //--TODO
        }

        public void Load(ISaveData saveData)
        {
            throw new NotImplementedException(); //--TODO
        }

        protected List<IGameCamera> Cameras = new();

        public void AddPet(IGameCamera pet)
        {
            AddCameraInternal(pet);
            GameUtils.AddPet(pet, Cameras);
        }

        public bool RemovePet(IGameCamera pet)
        {
            RemoveCameraInternal(pet);
            return GameUtils.RemovePet(pet, Cameras);
        }


        protected virtual void AddCameraInternal(IGameCamera cam)
        {
        }

        protected virtual void RemoveCameraInternal(IGameCamera cam)
        {
            
        }

        private IEnumerable<IGameCamera> pets;


        IEnumerable<IGameCamera> IOwnerOf<IGameCamera>.Pets => pets;

        public IEnumerable<object> AbsolutePets => (Cameras);
        public virtual void Tick()
        {
        }
    }
}