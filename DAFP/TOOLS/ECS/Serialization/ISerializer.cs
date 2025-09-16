using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISerializer
    {
        public Dictionary<string, object> Save(IEntity obj);
        public void Load(Dictionary<string, object> save, IEntity ent);

        public string GetDomainName();

        public const string ENT_BASIC_DATA_NAME = "EntBasic";
        public const string ENT_ADDITIONS_DATA_NAME = "EntData";
        public const string ENT_COMPONENTS_DATA_NAME = "ComponentsData";
        public const string STAT_DEFAULT = "StatDefault";
        public const string STAT_MAX = "StatMax";
        public const string STAT_MIN = "StatMin";
        public const string STAT_VAL = "StatValue";

        public static void AddTransformData(Dictionary<string, object> data, Transform t)
        {
            data["Position"] = t.position;
            data["Rotation"] = t.rotation;
            data["Scale"] = t.localScale;
        }

        public static object TryDeserializeToConcrete(JObject obj)
        {
            // Step 2.1: If JSON contains a $type hint, use it
            if (obj.TryGetValue("$type", out var typeToken) && typeToken.Type == JTokenType.String)
            {
                var typeName = typeToken.ToString();
                var type = Type.GetType(typeName, throwOnError: false);
                if (type != null)
                    return obj.ToObject(type);
            }

            // Step 2.2: Detect Quaternion shape (x, y, z, w)
            if (obj.TryGetValue("x", out var qx) &&
                obj.TryGetValue("y", out var qy) &&
                obj.TryGetValue("z", out var qz) &&
                obj.TryGetValue("w", out var qw) &&
                (qx.Type == JTokenType.Float || qx.Type == JTokenType.Integer))
            {
                return new Quaternion(
                    qx.ToObject<float>(),
                    qy.ToObject<float>(),
                    qz.ToObject<float>(),
                    qw.ToObject<float>()
                );
            }

            // Step 2.3: Detect Vector3 shape (x, y, z)
            if (obj.TryGetValue("x", out var vx) &&
                obj.TryGetValue("y", out var vy) &&
                obj.TryGetValue("z", out var vz) &&
                (vx.Type == JTokenType.Float || vx.Type == JTokenType.Integer))
            {
                return new Vector3(
                    vx.ToObject<float>(),
                    vy.ToObject<float>(),
                    vz.ToObject<float>()
                );
            }

            // Step 2.4: Detect Vector2 shape (x, y)
            if (obj.TryGetValue("x", out var sx) &&
                obj.TryGetValue("y", out var sy) &&
                (sx.Type == JTokenType.Float || sx.Type == JTokenType.Integer))
            {
                return new Vector2(
                    sx.ToObject<float>(),
                    sy.ToObject<float>()
                );
            }

            // Step 2.5: Detect Color shape (r, g, b, a)
            if (obj.TryGetValue("r", out var cr) &&
                obj.TryGetValue("g", out var cg) &&
                obj.TryGetValue("b", out var cb) &&
                obj.TryGetValue("a", out var ca) &&
                (cr.Type == JTokenType.Float || cr.Type == JTokenType.Integer))
            {
                return new Color(
                    cr.ToObject<float>(),
                    cg.ToObject<float>(),
                    cb.ToObject<float>(),
                    ca.ToObject<float>()
                );
            }

            // Step 2.6: Fallback to a generic dictionary
            return obj;
        }

        public static void DefaultLoad(object data, object sv)
        {
            if (data == null)
                return;

            if (sv is ISavable saveable)
            {
                saveable.Load(data as Dictionary<string, object>);
            }
        }

        public static Dictionary<string, object> DefaultSave(object sv)
        {
            var data = new Dictionary<string, object>();
            if (sv is ISavable saveable)
            {
                data.AddSave(saveable.Save());
            }

            return data;
        }
    }
}