using System;
using System.Collections;
using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Serialization;
using SKUnityToolkit.SerializableDictionary;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalCooldownController : EntityComponent, ISavable
    {
        [ReadOnly] [SerializeField] private SerializableHashSet<Cooldown> Cooldowns = new();

        public void RegisterCooldown(Cooldown down)
        {
            Cooldowns.Add(down);
        }

        protected override void OnTick()
        {
            foreach (var _cooldown in Cooldowns)
            {
                _cooldown.safeUpdateTimer(EntityComponentTicker.DeltaTime);
            }
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnStart()
        {
        }

        public Dictionary<string, object> Save()
        {
            var save = new Dictionary<string, object>();
            foreach (var _cooldown in Cooldowns)
            {
                save.Add(_cooldown.Name, _cooldown.Save());
            }

            return save;
        }

        public void Load(Dictionary<string, object> save)
        {
            foreach (var _cooldown in Cooldowns)
            {
                if (save.TryGetValue(_cooldown.Name, out var _value))
                {
                    _cooldown.Load(_value as Dictionary<string, object>);
                }
            }
            
        }

    }

    // [System.Serializable]
    // public struct CoolDown : IEquatable<CoolDown>, INameable
    // {
    //     [field: SerializeField] public string Name { get; set; }
    //     public FiniteTimer Timer;
    //
    //     public CoolDown(string name, FiniteTimer timer)
    //     {
    //         Name = name;
    //         Timer = timer;
    //     }
    //
    //     public bool Equals(CoolDown other)
    //     {
    //         return Name == other.Name && Timer.Equals(other.Timer);
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         return obj is CoolDown other && Equals(other);
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return HashCode.Combine(Name, Timer);
    //     }
    // }
}