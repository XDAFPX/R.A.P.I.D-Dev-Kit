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
        private readonly GlobalStates states;

        [Inject]
        public UniversalGlobalSettingsSaveSystem(GlobalStates states)
        {
            this.states = states;
        }

        public void SaveAll(ISerializationService saveService, ISerializer serializer, IMetaSerializer metaSerializer)
        {
            Dictionary<string, object> Data = new();

            Dictionary<string, object> _metaData = metaSerializer.SaveMetaData();
            List<IGlobalSettingsSavable> savableStates = states.GetStates()
                .OfType<IGlobalSettingsSavable>()
                .ToList();
            savableStates.Sort(new GenericComparerICompareable<IGlobalSettingsSavable>());
            foreach (var _base in savableStates)
            {
                Data.Add(_base.GetType().FullName, _base.Save());
            }

            Data.Add("Meta", _metaData);
            saveService.Save(Data, "Settings", "Default", "SettingsSave.Save");
        }

        public async Task LoadAll(ISerializationService saveService, ISerializer serializer,
            IMetaSerializer metaSerializer, Action OnEnd)
        {
            Dictionary<string, object> Data =
                saveService.Load("Settings", "Default", "SettingsSave.Save");


            if (Data.TryGetValue("Meta", out var _o1))
            {
                var metadata = _o1 as Dictionary<string, object>;
                await metaSerializer.LoadMetaData(metadata);
            }

            List<IGlobalSettingsSavable> savableStates = states.GetStates()
                .OfType<IGlobalSettingsSavable>()
                .ToList();
            savableStates.Sort(new GenericComparerICompareable<IGlobalSettingsSavable>());
            foreach (var _globalStateHandlerBase in savableStates)
            {
                if (Data.TryGetValue(_globalStateHandlerBase.GetType().FullName, out var oo))
                {
                    _globalStateHandlerBase.Load(oo as Dictionary<string, object>);
                }
            }

            OnEnd?.Invoke();
        }

        public SerializationBus Bus { get; } = new();
    }
}