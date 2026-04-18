using System.Collections.Generic;
using BandoWare.GameplayTags;
using DAFP.TOOLS.Common;
using SKUnityToolkit.SerializableDictionary;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    [System.Serializable]
    public class HurtGroup<T> : IOwnerOf<HurtBox<T>>, IPetOf<IHurtBoxController<T>, HurtGroup<T>>, INameable
    {
        [field : SerializeField]public string Name { get; set; }

        [SerializeField] private SerializableHashSet<HurtBox<T>> Group;


        [SerializeField] private SerializableInterface<IHaveGameplayTag> Tag;

        public HurtGroup(HashSet<HurtBox<T>> group, IHaveGameplayTag tag)
        {
            this.Group = new SerializableHashSet<HurtBox<T>>();
            Group.UnionWith(group);
            
            Tag = new SerializableInterface<IHaveGameplayTag>(tag);
        }

        public GameplayTagContainer RealTag => Tag.Value.GameplayTag;
        public IEnumerable<HurtBox<T>> Pets => Group;

        public void AddPet(HurtBox<T> pet)
        {
            Group.Add(pet);
        }

        public bool RemovePet(HurtBox<T> pet)
        {
            Group.Remove(pet);
            return true;
        }

        public List<IHurtBoxController<T>> Owners { get; } = new();
    }
}