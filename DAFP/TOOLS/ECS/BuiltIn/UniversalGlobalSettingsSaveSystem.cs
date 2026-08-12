using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalGlobalSettingsSaveSystem : IGlobalSettingsSaveSystem
    {
        private readonly IEnumerable<IGlobalStateHandlerBase> states;
        private readonly IEnumerable<GlobalBlackBoard> theblacks;

        [Inject]
        public UniversalGlobalSettingsSaveSystem(IEnumerable<IGlobalStateHandlerBase> states,
            IEnumerable<GlobalBlackBoard> theblacks)
        {
            this.states = states;
            this.theblacks = theblacks;
        }

        public void LoadAll(ISerializationDataService saveDataService, ISerializer serializer,
            ISerializer<GameMetaData> metaSerializer, int slot)
        {
            throw new NotImplementedException();
        }

        public void SaveAll(ISerializationDataService saveDataService, ISerializer serializer,
            ISerializer<GameMetaData> metaSerializer, int slot)
        {
            throw new NotImplementedException();
        }

        public void DeleteSave(ISerializationDataService dataService, int slot)
        {
            throw new NotImplementedException();
        }


        // public void SaveAll(ISerializationDataService saveDataService, ISerializer<IEntity> serializer,
        //     ISerializer<GameMetaData> metaSerializer,
        //     int slot)
        // {
        //     throw new NotImplementedException();
        //     Dictionary<string, object> Data = new();
        //
        //     var _metaData = metaSerializer.SaveMetaData(Data);
        //     var savableStates = states
        //         .OfType<IGlobalSettingsSavable>()
        //         .ToList();
        //     savableStates.Sort(new GenericComparerICompareable<IGlobalSettingsSavable>());
        //
        //     foreach (var _base in savableStates) Data.Add(_base.GetType().FullName, _base.Save());
        //
        //     var _blackBoards = theblacks
        //         .OfType<IGlobalSettingsSavable>()
        //         .ToList();
        //     _blackBoards.Sort();
        //     foreach (var _board in _blackBoards)
        //     {
        //         var _fullName = _board.GetType().FullName;
        //         if (_fullName != null)
        //             Data[_fullName] = _board.Save();
        //     }
        //
        //
        //     Data.Add("Meta", _metaData);
        //     saveDataService.Save(Data, "Settings", $"Default{slot}", "SettingsSave.Save");
        // }
        //
        // public void DeleteSave(ISerializationDataService dataService, int slot)
        // {
        //     dataService.DeleteSave("Settings", $"Default{slot}", "SettingsSave.Save");
        // }
        //
        // public void TryChangeCurrentScene(ISerializationDataService serializationDataService,
        //     IMetaSerializer serializer,
        //     int scene, int slot)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task LoadAll(ISerializationDataService saveDataService, ISerializer<IEntity> serializer,
        //     IMetaSerializer metaSerializer, Action OnEnd, int slot)
        // {
        //     var Data =
        //         saveDataService.Load("Settings", $"Default{slot}", "SettingsSave.Save");
        //
        //
        //     if (Data.TryGetValue("Meta", out var _o1))
        //     {
        //         var metadata = _o1 as Dictionary<string, object>;
        //         await metaSerializer.LoadMetaData(metadata);
        //     }
        //
        //     var savableStates = states
        //         .OfType<IGlobalSettingsSavable>()
        //         .ToList();
        //     savableStates.Sort(new GenericComparerICompareable<IGlobalSettingsSavable>());
        //     foreach (var _globalStateHandlerBase in savableStates)
        //         if (Data.TryGetValue(_globalStateHandlerBase.GetType().FullName, out var oo))
        //             _globalStateHandlerBase.Load(new GenericSaveData(oo as Dictionary<string, object>));
        //
        //
        //     var _blackBoards = theblacks
        //         .OfType<IGlobalSettingsSavable>()
        //         .ToList();
        //     _blackBoards.Sort();
        //     foreach (var _board in _blackBoards)
        //     {
        //         var _fullName = _board.GetType().FullName;
        //         if (Data.TryGetValue(_fullName, out var oo))
        //             _board.Load(new GenericSaveData(oo as Dictionary<string, object>));
        //     }
        //
        //
        //     OnEnd?.Invoke();
        // }
    }
}