using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DAFP.GAME.Essential;
using DAFP.TOOLS.Common.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace DAFP.TOOLS.Common.SVNL
{
    public class SaveManager : Manager<SaveManager>
    {
    
 

    
    
        public void SaveUniversal()
        {
            // SaveSaveSlot(TrySaveTheGame(CurrentSaveSlot.Slot));
        }
        public void LoadUniversal()
        {
            FullyLoadGame(CurrentSaveSlot.Slot);
        }
    
        public List<SavebleObjectRuntime> CurrentObjects = new List<SavebleObjectRuntime>();
        public SaveSlot CurrentSaveSlot;
        public void RegisterObject(SavebleObjectRuntime obj)
        {
            CurrentObjects.Add(obj);
        }
        // public SaveSlot TrySaveTheGame(int slot)
        // {
        //     List<SavebleObject> _objs = new List<SavebleObject>();
        //
        //     CurrentObjects.RemoveAll((obj) => { return obj == null; });
        //
        //     CurrentObjects.ForEach((obj) => { SubscribeISave(obj.gameObject, obj);  _objs.Add(obj.Save()); });
        //
        //     return new SaveSlot($"CGS_{slot}", slot, new MapSave(SceneManager.GetActiveScene().name, _objs.ToArray()), GetCurrentPlayerData());
        // }
        public void FullyLoadGame(int slot)
        {
            var _path = Application.persistentDataPath + "/Saves";
            string[] _files = Directory.GetFiles(_path);
            SaveSlot _savedgame = null;
            foreach (string _file in _files) {
                SaveSlot _saveSlot = DeSerializeSlot(_file);
                if (_saveSlot != null) {
                    if (_saveSlot.Slot == slot) {
                        _savedgame = _saveSlot;
                        break;
                    }
    
                }
            }
            if (_savedgame == null) {
                Debug.LogError($"Failed to find any saves for id {slot} or just a json error");
                return;
            }
            LoadGame(_savedgame);
    
        }
        public void LoadGame(SaveSlot savedgame)
        {
            Debug.Log($"Loading... {savedgame.Name} for slot {savedgame.Slot}");
    
            CurrentSaveSlot = savedgame;
            LoadScene(savedgame);
    
        }
        private void LoadScene(SaveSlot savedgame)
        {
            SceneManager.LoadSceneAsync(savedgame.MapSave.Name).completed+=LoadObjectAndPlayerData;
        }
    
        private void LoadObjectAndPlayerData(AsyncOperation operation)
        {
            Debug.Log("Loading Objects Data");
            // StartCoroutine(WaitAndLoadStuff());
    
        }
        // private IEnumerator WaitAndLoadStuff()
        // {
        //     for (int _i = 0; _i < 2; _i++)
        //     {
        //         yield return null;
        //     }
        //     UIManager.Singleton.HealthBar.value = CurrentSaveSlot.PlayerPlot.Health;
        //     UIManager.Singleton.ShieldBar.value = CurrentSaveSlot.PlayerPlot.Shield;
        //     UIManager.Singleton.AddBattery(CurrentSaveSlot.PlayerPlot.Money);
        //     GameSystem.GetPlayer().transform.position = CurrentSaveSlot.PlayerPlot.Pos;
        //     GameSystem.GetPlayer().transform.rotation = Quaternion.Euler(CurrentSaveSlot.PlayerPlot.Rot);
        //     CurrentObjects.RemoveAll((obj) => { return obj == null; });
        //     foreach (var _obj in CurrentSaveSlot.MapSave.Objects)
        //     {
        //         Debug.Log($"Trying to Load {_obj.Name} with ID: {_obj.ID}");
        //         var _c = CurrentObjects.Find((SavebleObjectRuntime o) => { return o.UniqueID == _obj.ID; });
        //
        //         if (_c!=null)
        //         {
        //             _c.LoadData(_obj);
        //             continue;
        //         }
        //         
        //
        //         //--------------------------------------------------------------
        //
        //         try
        //         {
        //             var _path = _obj.GetInstanceResourcePath();
        //             if (_path != null)
        //             {
        //                 Debug.Log("Loaded Resource: " + _path);
        //                 GameObject _inst = (GameObject)Instantiate(Resources.Load(_path));
        //                 if (_inst.TryGetComponent<SavebleObjectRuntime>(out var _save_inst))
        //                 {
        //                     SubscribeISave(_inst, _save_inst);
        //
        //                     _save_inst.LoadData(_obj);
        //                 }
        //                 else
        //                 {
        //                     _inst.transform.position = _obj.Pos;
        //                     _inst.transform.rotation = Quaternion.Euler(_obj.Rot);
        //                     _inst.transform.localScale = _obj.Size;
        //                 }
        //
        //             }
        //         }
        //         catch (Exception _e)
        //         {
        //
        //             Debug.LogError($"Instancable Load Error. Failed trying load {_obj.Name} with Instancable  Id of {_obj.GetInt("Instancable")} Also here is the exeption: {_e.Message} ");
        //         }
        //     }
        //
        //
        //
        // }
    
        private static void SubscribeISave(GameObject inst1, SavebleObjectRuntime inst2)
        {
            if (inst1.TryGetComponent<ISaveSubsriber>(out var _subsriber))
            {
                inst2.EOnLoadData += _subsriber.LoadData;
                inst2.EOnSaveAdditionalData += _subsriber.SaveAdditonalData;
            }
        }
    
        private SaveSlot DeSerializeSlot(string path)
        {
            SaveSlot _slto = null;
            try
            {
                
                _slto = JsonUtility.FromJson<SaveSlot>(File.ReadAllText(path));
            }
            catch (System.Exception)
            {
    
                Debug.LogError($"Failed To Load File {path}");
            }
            return _slto;
        }
        /*private PlayerPlotSave GetCurrentPlayerData()
        {
            Player _pl = GameSystem.GetPlayer();
            return new PlayerPlotSave(UIManager.Singleton.GetHealth(), UIManager.Singleton.HealthBar.maxValue, UIManager.Singleton.ShieldBar.value, UIManager.Singleton.ShieldBar.maxValue, _pl.MaximumWeapon, UIManager.Singleton.Batteries,_pl.transform.position,_pl.transform.rotation.eulerAngles);
        }*/
    
        public void SaveSaveSlot(SaveSlot slot)
        {
            var _path = Application.persistentDataPath;
    
            if(slot == null)
            {
                Debug.LogError("Uhhh your save is bugged and will be ANIGILATED AHHAHAHAHHAHAHAHHA");
                slot = GenerateDefaultSave(0);
            }
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
            string _save = _path + $"/Saves/{slot.Name}Save.Save";
            File.WriteAllText(_save, JsonUtility.ToJson(slot));
            Debug.Log($"Saved: {_save}");
            
        }
    
        public SaveSlot GenerateDefaultSave(int slotid)
        {
            return new SaveSlot($"CGS_{slotid}", slotid, new MapSave("Beggining", new SavebleObject[0]),new PlayerPlotSave(100,100,75,75,0,0,Vector3.zero,Vector3.zero));
        }
    }
    [System.Serializable]
    public class SaveSlot
    {
        [FormerlySerializedAs("name")] public string Name;
        [FormerlySerializedAs("slot")] public int Slot;
        public MapSave MapSave;
        public PlayerPlotSave PlayerPlot;
    
        public SaveSlot(string name, int slot, MapSave mapSave, PlayerPlotSave playerPlot)
        {
            this.Name = name;
            this.Slot = slot;
            MapSave = mapSave;
            PlayerPlot = playerPlot;
        }
    }
    [System.Serializable]
    public class PlayerPlotSave
    {
        public float Health;
        public float MaxHealth;
        public float Shield;
        public float MaxShield;
        public int MaxWeapon;
        public int Money;
        [FormerlySerializedAs("pos")] public Vector3 Pos;
        [FormerlySerializedAs("rot")] public Vector3 Rot;
    
        public PlayerPlotSave(float health, float maxHealth, float shield, float maxShield, int maxWeapon, int money, Vector3 pos, Vector3 rot)
        {
            Health = health;
            MaxHealth = maxHealth;
            Shield = shield;
            MaxShield = maxShield;
            MaxWeapon = maxWeapon;
            Money = money;
            this.Pos = pos;
            this.Rot = rot;
        }
    }
    [System.Serializable]
    public class MapSave
    {
        [FormerlySerializedAs("name")] public string Name;
        public SavebleObject[] Objects;
    
        public MapSave(string name, SavebleObject[] objects)
        {
            this.Name = name;
            Objects = objects;
        }
    }
}