using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BigData;
using FluentResults;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class EntityDefaultSerializer : EntityConfigSerializer

    {
        public override Dictionary<string, object> OnAddAdditionalStats(IStatBase stat)
        {
            var dict = new Dictionary<string, object>();
            dict.Add(ISerializer<IEntity>.STAT_VAL, stat.GetAbsoluteValue());
            return dict;
        }

        public override void ProcessAdditionalStatInfo(IStatBase stat, Dictionary<string, object> data)
        {
            if (data.TryGetValue(ISerializer<IEntity>.STAT_VAL, out var _value)) stat.SetAbsoluteValue(_value);
        }

        public override Result DeSerialize(ISaveData save, IEntity ent)
        {
            throw new NotImplementedException();
            if (ent == null)
                //TODO Handle dynamicly spawned Assets
                return Result.Fail(new ExceptionalError(new NotImplementedException()));

            // if (save == null)
            // {
            //     GameObject.Destroy(ent.GetWorldRepresentation());
            //     
            //     return Result.Ok();
            // }
            //
            // save.ApplyConcreteDeserialization();
            // if (save.TryGetValue(ent.GetType().FullName + "." + ISerializer<IEntity>.ENT_BASIC_DATA_NAME,
            //         out var _value))
            // {
            //     var transform_save = _value as Dictionary<string, object>;
            //
            //
            //     if (transform_save.TryGetValue("Position", out var _o))
            //         if (_o is Vector3 vv)
            //             ent.GetWorldRepresentation().transform.localPosition = vv;
            //
            //     if (transform_save.TryGetValue("Scale", out var _s))
            //         if (_s is Vector3 vv)
            //             ent.GetWorldRepresentation().transform.localScale = vv;
            //
            //     if (transform_save.TryGetValue("Rotation", out var _q))
            //         if (_q is Quaternion vv)
            //             ent.GetWorldRepresentation().transform.rotation = vv;
            // }
            //
            // base.DeSerialize(save, ent);
        }

        public override Result<ISaveData> Serialize(IEntity ent)
        {
            throw new NotImplementedException();
            // var save = base.Serialize(ent);
            //
            // var transform_save = new Dictionary<string, object>();
            //
            // transform_save.Add("Position", ent.GetWorldRepresentation().transform.localPosition);
            // transform_save.Add("Rotation", ent.GetWorldRepresentation().transform.rotation);
            // transform_save.Add("Scale", ent.GetWorldRepresentation().transform.localScale);
            // transform_save.Add("Name", ent.GetWorldRepresentation().name);
            // transform_save.Add("ID", ent.ID);
            //
            // save.Add(ent.GetType().FullName + "." + ISerializer<IEntity>.ENT_BASIC_DATA_NAME, transform_save);
            // return save;
        }

        protected override void SaveStat(ref Dictionary<string, object> _statdata, IStatBase stat)
        {
            _statdata = OnAddAdditionalStats(stat);
        }
    }
}