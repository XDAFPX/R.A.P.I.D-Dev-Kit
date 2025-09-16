using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class ConfigSerializer : ISerializer
    {
        public virtual void OnDesirializeAdditionalStats(IStatBase stat, Dictionary<string, object> data)
        {
        }

        public virtual Dictionary<string, object> OnAddAdditionalStats(IStatBase stat)
        {
            return new Dictionary<string, object>();
        }

        public virtual Dictionary<string, object> Save(IEntity ent)
        {
            var data = new Dictionary<string, object>();
            data.Add($"{ent.GetType().FullName}.{ISerializer.ENT_ADDITIONS_DATA_NAME}", ISerializer.DefaultSave(ent));
            var _compdata = new Dictionary<string, object>();
            foreach (var _componentsValue in ent.Components.Values)
            {
                if (_componentsValue is IStatBase stat)
                {
                    var _statdata = new Dictionary<string, object>();
                    SaveStat(ref _statdata, stat);
                    _compdata.Add(stat.GetType().FullName + ".Stats", _statdata);
                }

                if (_componentsValue is ISavable sv)
                {
                    _compdata.Add($"{_componentsValue.GetType().FullName}", sv.Save());
                }
            }

            data.Add(ISerializer.ENT_COMPONENTS_DATA_NAME, _compdata);
            return data;
        }

        protected virtual void SaveStat(ref Dictionary<string, object> _statdata, IStatBase stat
            )
        {
            _statdata.Add($"{ISerializer.STAT_DEFAULT}", stat.GetAbsoluteDefault());
            _statdata.Add($"{ISerializer.STAT_MIN}", stat.GetAbsoluteMin());
            _statdata.Add($"{ISerializer.STAT_MAX}", stat.GetAbsoluteMax());
            _statdata.AddSave(OnAddAdditionalStats(stat));
        }

        public virtual void Load(Dictionary<string, object> save, IEntity ent)
        {
            save.ApplyConcreteDeserialization();

            ISerializer.DefaultLoad(
                save.GetValueOrDefault($"{ent.GetType().FullName}.{ISerializer.ENT_ADDITIONS_DATA_NAME}"), ent);
            if (save.TryGetValue(ISerializer.ENT_COMPONENTS_DATA_NAME, out var obj))
            {
                Dictionary<string, object> comp_data = obj as Dictionary<string, object>;
                foreach (var _entityComponent in ent.Components)
                {
                    if (_entityComponent.Value is IStatBase stat &&
                        comp_data.TryGetValue(_entityComponent.Key.FullName + ".Stats", out var _value))
                    {
                        Dictionary<string, object> stat_data = _value as Dictionary<string, object>;

                        LoadStat(stat_data, stat);
                    }

                    if (comp_data.TryGetValue(_entityComponent.Key.FullName, out var _o))
                    {
                        if (_entityComponent.Value is ISavable saveable)
                        {
                            saveable.Load(_o as Dictionary<string, object>);
                        }
                    }
                }
            }
        }

        protected virtual void LoadStat(Dictionary<string, object> stat_data, IStatBase stat)
        {
            if (stat_data.TryGetValue(ISerializer.STAT_DEFAULT, out var _value1))
            {
                stat.SetAbsoluteDefault(_value1);
            }

            if (stat_data.TryGetValue(ISerializer.STAT_MAX, out var _value2))
            {
                stat.SetAbsoluteMax(_value2);
            }

            if (stat_data.TryGetValue(ISerializer.STAT_MIN, out var _value3))
            {
                stat.SetAbsoluteMin(_value3);
            }

            OnDesirializeAdditionalStats(stat, stat_data);
        }

        public string GetDomainName()
        {
            return "Config";
        }
    }
}