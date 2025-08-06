using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.Common.SVNL
{
    [System.Serializable]
    public class SavebleObject
    {
        [FormerlySerializedAs("NAME")] public string Name;
        public string ID;
        [FormerlySerializedAs("pos")] public Vector3 Pos;
        [FormerlySerializedAs("rot")] public Vector3 Rot;
        [FormerlySerializedAs("size")] public Vector3 Size;
        [FormerlySerializedAs("DATA")] public SaveData[] Data;
    
        public string GetInstanceResourcePath()
        {
            if (this.GetInt("Instancable") == -1)
                return null;
            if (PATH_LOOK_UP_TABLE.TryGetValue(this.GetInt("Instancable"),out string _val))
            {
                return "Instancables/"+_val;
            }
            return null;
        }
        public SavebleObject(string nAme, string iD, Vector3 pos, Vector3 rot, Vector3 size, SaveData[] dAta)
        {
            Name = nAme;
            ID = iD;
            this.Pos = pos;
            this.Rot = rot;
            this.Size = size;
            Data = dAta;
        }
        public static float BoolToFloat(bool b)
        {
            if(b)
                return 1;
            else return 0;
        }
        public static bool FloatToBool(float f)
        {
            return f > 0;
        }
        public float GetFloat(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            return Data.FirstOrDefault((x) => x.Key == key).Value;
        }
        public bool GetBool(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return FloatToBool(Data.FirstOrDefault((x) => x.Key == key).Value);
        }
        public int GetInt(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            return Mathf.RoundToInt( Data.FirstOrDefault((x) => x.Key == key).Value);
        }
        public static Dictionary<int,string> PATH_LOOK_UP_TABLE = new Dictionary<int, string>() { {0, "Battery" } };
    
    }
    public static class SaveUtils
    {
        public static SaveData[] PutFloat(this SaveData[] data, string key, float value)
        {
            if (data == null)
                data = new SaveData[0];
            var _l = data.ToList();
            _l.Add(new SaveData(key, value));
            data = _l.ToArray();
            return data;
        }
        public static SaveData[] PutBool(this SaveData[] data, string key, bool value)
        {
            if(data==null)
                data = new SaveData[0];
            var _l = data.ToList();
            _l.Add(new SaveData(key, SavebleObject.BoolToFloat(value)));
            data = _l.ToArray();
            return data;
        }
        public static SaveData[] PutInt(this SaveData[] data, string key, int value)
        {
            if (data == null)
                data = new SaveData[0];
            var _l = data.ToList();
            _l.Add(new SaveData(key, Mathf.RoundToInt(value)));
            data = _l.ToArray();
            return data;
        }
    }
    public interface ISaveSubsriber
    {
        void LoadData(SavebleObject data);
    
    
        SaveData[] SaveAdditonalData(SavebleObject data);
    
    }

}