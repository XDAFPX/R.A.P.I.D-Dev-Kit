using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common;
using SKUnityToolkit.SerializableDictionary;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalCooldownController : EntityComponent
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