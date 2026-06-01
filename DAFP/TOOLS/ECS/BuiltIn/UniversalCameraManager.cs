using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;
using Zenject;
#if CINEMAMACHINE
using Unity.Cinemachine;
#endif

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCameraManager : ScriptableObject, ICameraManager, IInitializable
    {
        [Inject] private World world;
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

            ManageSubjects(subjects).Forget();
        }

        public void RemoveSubject(IEntity ent)
        {
            if (!subjects.Contains(ent)) return;
            subjects.Remove(ent);

            ManageSubjects(subjects).Forget();
        }

        public DelegateSubjectPolicy Policy = DelegateSubjectPolicy.AllForOne;

        public enum DelegateSubjectPolicy
        {
            AllForOne,
            SplitScreen,
            Multiplayer
        }

        public async UniTask
            ManageSubjects(IEnumerable<IEntity> s) //--one rule. No subject should be in two cameras at ones
        {
            reset_cams();
            switch (Policy)
            {
                case DelegateSubjectPolicy.AllForOne:
                    Cams.FirstOrDefault()?.UpdateSubjects(s);
                    break;
                case DelegateSubjectPolicy.SplitScreen:
                    var _entities = s as IEntity[] ?? s.ToArray();
                    var diff = _entities.Length - Cams.Count;
                    if (diff > 0)
                    {
                        for (int i = 0; i < diff; i++)
                        {
                            var _tamplate =
                                Cams.OfType<IEntity>().FirstOrDefault(); // ?? world.Create<CinemaCa>() --TODO FISXX
                            var _clone = await world.Clone(_tamplate) as IGameCamera;
                            Cams.Add(_clone);
                        }
                    }

                    for (int i = 0; i < _entities.Length; i++)
                    {
                        Cams[i].Rect = get_rect(i, _entities.Length);
                        Cams[i].UpdateSubjects(_entities[i].ToEnumerable());
                    }

                    break;
                case DelegateSubjectPolicy.Multiplayer
                    : //--Since only a local player needs a "Real" cameras all others can be dummies 

                    var _enumerable = s as IEntity[] ?? s.ToArray();
                    var _ent = _enumerable.Players(GameUtils.PlayerSelectionPolicy.SingleOut).FirstOrDefault()?.Body ??
                               _enumerable.FirstOrDefault();
                    if (_ent == null)
                        return;
                    Cams.FirstOrDefault()?.UpdateSubjects(_ent.ToEnumerable());

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void reset_cams()
        {
            Cams.ForEach((camera =>
            {
                camera.UpdateSubjects(Array.Empty<IEntity>());
                camera.Rect = new Rect(0, 0, 1, 1);
            }));
        }

        private Rect get_rect(int index, int total)
        {
            int cols = Mathf.CeilToInt(Mathf.Sqrt(total));
            int rows = Mathf.CeilToInt((float)total / cols);

            float w = 1f / cols;
            float h = 1f / rows;

            int col = index % cols;
            int row = index / cols;

            // last row may have fewer cells — center them
            int lastRowCount = total % cols == 0 ? cols : total % cols;
            float xOffset = row == rows - 1 ? (cols - lastRowCount) * w * 0.5f : 0f;

            return new Rect(col * w + xOffset, 1f - (row + 1) * h, w, h);
        }

        public ISaveData Save()
        {
            throw new NotImplementedException(); //--TODO
        }

        public void Load(ISaveData saveData)
        {
            throw new NotImplementedException(); //--TODO
        }

        protected List<IGameCamera> Cams = new();

        public void AddPet(IGameCamera pet)
        {
            AddCameraInternal(pet);
            GameUtils.AddPet(pet, Cams);
            ManageSubjects(subjects).Forget();
        }

        public bool RemovePet(IGameCamera pet)
        {
            RemoveCameraInternal(pet);
            reset_cams();
            var res = GameUtils.RemovePet(pet, Cams);
            ManageSubjects(subjects).Forget();
            return res;
        }


        protected virtual void AddCameraInternal(IGameCamera cam)
        {
        }

        protected virtual void RemoveCameraInternal(IGameCamera cam)
        {
        }

        private IEnumerable<IGameCamera> pets;


        IEnumerable<IGameCamera> IOwnerOf<IGameCamera>.Pets => pets;

        public IEnumerable<object> AbsolutePets => (Cams);

        public virtual void Tick()
        {
        }
    }
}