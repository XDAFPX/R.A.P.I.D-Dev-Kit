using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.Common.SVNL
{
    public class SavebleObjectRuntime : MonoBehaviour
    {
    
        private SavebleObject data;
        public delegate SaveData[] SaveAdditionalData(SavebleObject data);
        public delegate void LoadAllData(SavebleObject data);
        public event SaveAdditionalData EOnSaveAdditionalData;
        public event LoadAllData EOnLoadData;
        public bool LoadLocation = true;
        public int ResourceID = -1;
        public string UniqueID;
    
    
        private void Reset()
        {
            UniqueID = Guid.NewGuid().ToString();
        }
        private void Start()
        {
            SaveManager.Singleton.RegisterObject(this);
        }
        public virtual SavebleObject Save()
        {
            data = new SavebleObject("","",Vector3.zero,Vector3.zero,Vector3.zero,null);
            data.Name = gameObject.name;
            data.ID = UniqueID;
            data.Pos = gameObject.transform.position;
            data.Rot = transform.rotation.eulerAngles;
            data.Size = transform.localScale;
            List<SaveData> _dd = new List<SaveData>();
            _dd.Add(new SaveData("Instancable",ResourceID));
            if (EOnSaveAdditionalData != null) // Check if there are any subscribers
            {
                foreach (SaveAdditionalData _subscriber in EOnSaveAdditionalData.GetInvocationList())
                {
                    SaveData[] _additionalData = _subscriber(data);
                    if (_additionalData != null)
                    {
                        _dd.AddRange(_additionalData); // Add additional data to the list
                    }
                }
            }
            _dd.Distinct();
    
    
    
    
    
    
            data.Data = _dd.ToArray();
    
    
    
    
            return data;
        }
    
        public virtual void LoadData(SavebleObject data)
        {
            this.data = data;
            if(EOnLoadData != null)
            {
                EOnLoadData.Invoke(this.data);
            }
            if(LoadLocation)
                SetWorldLocation(this.data);
            
        }
        public void SetWorldLocation(SavebleObject worldLocation)
        {
            transform.position = worldLocation.Pos;
            transform.localScale = worldLocation.Size;
            transform.rotation = Quaternion.Euler( worldLocation.Rot);
        }
    }
    [System.Serializable]
    public class SaveData
    {
        [FormerlySerializedAs("key")] public string Key;
        [FormerlySerializedAs("value")] public float Value;
    
        public SaveData(string key, float value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}