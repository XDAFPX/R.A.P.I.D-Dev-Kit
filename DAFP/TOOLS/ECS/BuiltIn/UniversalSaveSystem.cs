using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using PixelRouge.CsharpExtensionMethods;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEventBus;
using Zenject;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalSaveSystem : ISaveSystem
    {
        private readonly World world;
        private readonly IEnumerable<IGlobalStateHandlerBase> states;
        private readonly IEnumerable<GlobalBlackBoard> boards;

        [Inject]
        public UniversalSaveSystem(World world, IEnumerable<IGlobalStateHandlerBase> states,
            IEnumerable<GlobalBlackBoard> boards)
        {
            this.world = world;
            this.states = states;
            this.boards = boards;
        }

        public void SaveAll(ISerializationService saveService, ISerializer<IEntity> serializer,
            IMetaSerializer metaSerializer,
            int slot)
        {
            var _allData =
                saveService.Load(GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3);
            var _worldData = new Dictionary<string, object>();
            var _worldsData = new Dictionary<string, object>();
            var _gameStateData = new Dictionary<string, object>();
            var _globalBoardsData = new Dictionary<string, object>();
            var _presistantEnts = new Dictionary<string, object>();
            var _metaData =
                metaSerializer.SaveMetaData(_allData.GetValueOrDefault("Meta") as Dictionary<string, object>);

            foreach (var _entity in world.ENTITIES)
            {
                if (_entity.GetWorldRepresentation().GetComponent<NotSaveableEnt>() != null)
                    continue;
                if (_entity.GetWorldRepresentation().TryGetComponent(out PresistantEnt tt))
                {
                    _presistantEnts.AddOrSet(_entity.ID, serializer.Save(_entity));
                    continue;
                }

                _worldData.AddOrSet(_entity.ID, serializer.Save(_entity));
            }


            if (_allData.TryGetValue("Meta", out var _o))
            {
                var _metaSave = _o as Dictionary<string, object>;
                if (_metaSave.TryGetValue("PresistantEnts", out var _value1))
                {
                    var pre = _value1 as Dictionary<string, object>;
                    pre.AddSave(_presistantEnts);
                    _metaData.Add("PresistantEnts", pre);
                }
            }
            else
            {
                _metaData.Add("PresistantEnts", _presistantEnts);
            }

            var _savableStates = states
                .OfType<ISavable>()
                .Where(sv => sv is not IGlobalSettingsSavable) // exclude global‐settings savables
                .ToList();
            _savableStates.Sort(new GenericComparerICompareable<ISavable>());
            foreach (var _savableState in _savableStates)
            {
                var _fullName = _savableState.GetType().FullName;
                if (_fullName != null)
                    _gameStateData[_fullName] = _savableState.Save();
            }


            var _blackBoards = boards
                .Where(sv => sv is not IGlobalSettingsSavable) // exclude global‐settings savables
                .ToList();
            _blackBoards.Sort();
            foreach (var _board in _blackBoards)
            {
                var _fullName = _board.GetType().FullName;
                if (_fullName != null)
                    _globalBoardsData[_fullName] = _board.Save();
            }


            if (_allData.TryGetValue("Worlds", out var _value))
            {
                var worlds = _value as Dictionary<string, object>;
                worlds[SceneManager.GetActiveScene().buildIndex.ToString()] = _worldData;
            }
            else
            {
                _worldsData[SceneManager.GetActiveScene().buildIndex.ToString()] = _worldData;
                _allData["Worlds"] = _worldsData;
            }

            _allData["GameState"] = _gameStateData;
            _allData["GameData"] = _globalBoardsData;
            _allData["Meta"] = _metaData;
            saveService.Save(_allData, GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3);
            ((IEventBus)Bus).Send(new OnSaveMadeOrLoaded(
                saveService.GetFullSavePath(GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3), serializer,
                saveService,
                true));
        }

        public void DeleteSave(ISerializationService service, int slot)
        {
            service.DeleteSave(GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3);
        }

        public void TryChangeCurrentScene(ISerializationService serializationService, IMetaSerializer serializer,
            int scene, int slot)
        {
            var data = serializationService.Load(GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3);
            if (data.TryGetValue("Meta", out var _o1))
            {
                var metadata = _o1 as Dictionary<string, object>;
                serializer.TryChangeCurrentScene(metadata, scene);
                data["Meta"] = metadata;
            }

            serializationService.Save(data, GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3);
        }

        private (string, string, string) GetPath(int slot)
        {
            return ("DefaultSaves", $"Slot{slot}", "GameSave.Save");
        }

        public async Task LoadAll(ISerializationService saveService, ISerializer<IEntity> serializer,
            IMetaSerializer metaSerializer, Action OnEnd, int slot)
        {
            if (slot != -1)
                DeleteSave(saveService, -1);
            var _allData = new Dictionary<string, object>();
            _allData = saveService.Load(GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3);
            _allData.ApplyConcreteDeserialization();
            if (_allData.TryGetValue("Meta", out var _o1))
            {
                var metadata = _o1 as Dictionary<string, object>;
                await metaSerializer.LoadMetaData(metadata);
            }

            var _savableStates = states
                .OfType<ISavable>()
                .Where(sv => sv is not IGlobalSettingsSavable) // exclude global‐settings savables
                .ToList();
            _savableStates.Sort(new GenericComparerICompareable<ISavable>());
            if (_allData.TryGetValue("GameState", out var _o))
            {
                var _statedata = _o as Dictionary<string, object>;
                foreach (var _globalStateHandlerBase in _savableStates)
                {
                    var _fullName = _globalStateHandlerBase.GetType().FullName;
                    if (_fullName != null)
                        if (_statedata.TryGetValue(_fullName, out var _value))
                        {
                            _globalStateHandlerBase.Load(_value as Dictionary<string, object>);
                            _statedata.Remove(_fullName);
                        }
                }
            }


            var _blackBoards = boards
                .Where(sv => sv is not IGlobalSettingsSavable) // exclude global‐settings savables
                .ToList();
            _blackBoards.Sort();


            if (_allData.TryGetValue("GameData", out var _value3))
            {
                var _boardData = _value3 as Dictionary<string, object>;
                foreach (var _board in _blackBoards)
                {
                    var _fullName = _board.GetType().FullName;
                    if (_fullName != null)
                        if (_boardData.TryGetValue(_fullName, out var _value))
                        {
                            _board.Load(_value as Dictionary<string, object>);
                            _boardData.Remove(_fullName);
                        }
                }
            }


            await Task.Yield();
            await Task.Yield();
            if (_allData.TryGetValue("Worlds", out var _value1))
            {
                var _worldsdata = _value1 as Dictionary<string, object>;
                if (_worldsdata.TryGetValue(SceneManager.GetActiveScene().buildIndex.ToString(), out var _value))
                {
                    var _worlddata = _value as Dictionary<string, object>;


                    var _matchedKeys = new List<string>();

                    foreach (var _entity in world.ENTITIES)
                    {
                        var _key = _worlddata.Keys.ToList().Find(s => s == _entity.ID);

                        if (_entity.GetWorldRepresentation().GetComponent<NotSaveableEnt>() != null)
                        {
                            _matchedKeys.Add(_key);
                            continue;
                        }

                        if (_entity.GetWorldRepresentation().GetComponent<PresistantEnt>())
                        {
                            _matchedKeys.Add(_key);
                            continue;
                        }

                        if (_key != default)
                        {
                            serializer.Load(_worlddata.GetValueOrDefault(_key) as Dictionary<string, object>,
                                _entity);
                            _matchedKeys.Add(_key);
                        }
                        else
                        {
                            serializer.Load(null, _entity);
                        }
                    }

                    var _unMatched = _worlddata.Keys.ToList().Except(_matchedKeys).ToList();
                    foreach (var _unmatchedKey in _unMatched)
                        serializer.Load(_worlddata.GetValueOrDefault(_unmatchedKey) as Dictionary<string, object>,
                            null);
                }
            }

            if (_allData.TryGetValue("Meta", out var _o2))
            {
                var _metaSave = _o2 as Dictionary<string, object>;


                if (_metaSave.TryGetValue("PresistantEnts", out var value))
                {
                    var PresistantEnts = value as Dictionary<string, object>;

                    foreach (var _entity in world.ENTITIES)
                        if (_entity.GetWorldRepresentation().GetComponent<PresistantEnt>())
                            if (PresistantEnts.TryGetValue(_entity.ID, out var _value2))
                            {
                                var savedata = _value2 as Dictionary<string, object>;
                                serializer.Load(savedata, _entity);
                            }
                }
            }

            ((IEventBus)Bus).Send(new OnSaveMadeOrLoaded(
                saveService.GetFullSavePath(GetPath(slot).Item1, GetPath(slot).Item2, GetPath(slot).Item3), serializer,
                saveService,
                false));
            OnEnd?.Invoke();
        }

        public SerializationBus Bus { get; } = new();
    }
}