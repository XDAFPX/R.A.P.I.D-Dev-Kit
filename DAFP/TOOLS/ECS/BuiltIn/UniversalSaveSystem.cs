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
    public class UniversalSaveSystem : ISaveSystem //TODO Make the save file contain data for multiple scenes 
    {
        private readonly World world;
        private readonly GlobalStates states;

        [Inject]
        public UniversalSaveSystem(World world, GlobalStates states)
        {
            this.world = world;
            this.states = states;
        }

        public void SaveAll(ISerializationService saveService, ISerializer serializer, IMetaSerializer metaSerializer)
        {
            Dictionary<string, object> _allData = new Dictionary<string, object>();
            Dictionary<string, object> _worldData = new Dictionary<string, object>();
            Dictionary<string, object> _gameStateData = new Dictionary<string, object>();
            Dictionary<string, object> _metaData = metaSerializer.SaveMetaData();
            foreach (var _entity in world.ENTITIES)
            {
                _worldData.AddOrSet(_entity.ID, serializer.Save(_entity));
            }


            List<ISavable> _savableStates = states.GetStates()
                .OfType<ISavable>()
                .Where(sv => sv is not IGlobalSettingsSavable) // exclude global‐settings savables
                .ToList();
            _savableStates.Sort(new GenericComparerICompareable<ISavable>());
            foreach (var _savableState in _savableStates)
            {
                var _fullName = _savableState.GetType().FullName;
                if (_fullName != null)
                    _gameStateData.Add(_fullName, _savableState.Save());
            }

            _allData.Add("World", _worldData);
            _allData.Add("GameState", _gameStateData);
            _allData.Add("Meta", _metaData);
            saveService.Save(_allData, "DefaultSaves", "Slot1", "GameSave.Save");
            ((IEventBus)Bus).Send(new OnSaveMadeOrLoaded(
                saveService.GetFullSavePath("DefaultSaves", "Slot1", "GameSave.Save"), serializer, saveService, true));
        }


        public async Task LoadAll(ISerializationService saveService, ISerializer serializer,
            IMetaSerializer metaSerializer, Action OnEnd)
        {
            Dictionary<string, object> _allData = new Dictionary<string, object>();
            _allData = saveService.Load("DefaultSaves", "Slot1", "GameSave.Save");
            _allData.ApplyConcreteDeserialization();
            if (_allData.TryGetValue("Meta", out var _o1))
            {
                var metadata = _o1 as Dictionary<string, object>;
                await metaSerializer.LoadMetaData(metadata);
            }

            List<ISavable> _savableStates = states.GetStates()
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
                    {
                        if (_statedata.TryGetValue(_fullName, out var _value))
                        {
                            _globalStateHandlerBase.Load(_value as Dictionary<string, object>);
                            _statedata.Remove(_fullName);
                        }
                    }
                }
            }

            if (_allData.TryGetValue("World", out var _value1))
            {
                var _worlddata = _value1 as Dictionary<string, object>;

                List<string> _matchedKeys = new List<string>();
                await Task.Yield();
                await Task.Yield();
                if (world.ENTITIES.Count != 0)
                {
                    foreach (var _entity in world.ENTITIES)
                    {
                        var _key = _worlddata.Keys.ToList().Find((s => s == _entity.ID));
                        if (_key != default)
                        {
                            serializer.Load(_worlddata.GetValueOrDefault(_key) as Dictionary<string, object>, _entity);
                            _matchedKeys.Add(_key);
                        }
                        else
                        {
                            serializer.Load(null, _entity);
                        }
                    }

                    List<string> _unMatched = _worlddata.Keys.ToList().Except(_matchedKeys).ToList();
                    foreach (var _unmatchedKey in _unMatched)
                    {
                        serializer.Load(_worlddata.GetValueOrDefault(_unmatchedKey) as Dictionary<string, object>,
                            null);
                    }
                }
            }

            ((IEventBus)Bus).Send(new OnSaveMadeOrLoaded(
                saveService.GetFullSavePath("DefaultSaves", "Slot1", "GameSave.Save"), serializer, saveService, false));
            OnEnd?.Invoke();
        }

        public SerializationBus Bus { get; } = new();
    }
}